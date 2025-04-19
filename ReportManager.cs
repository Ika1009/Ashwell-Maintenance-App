using Ashwell_Maintenance;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;

public static class ReportManager
{
    /// <summary>
    /// Attempts to upload the report and its PDF. Throws on any failure.
    /// </summary>
    public static async Task UploadReportAsync(
        Enums.ReportType reportType,
        string reportName,
        Folder folder,
        Dictionary<string, string> reportData)
    {
        // 1) Upload raw report data
        HttpResponseMessage response = await ApiService.UploadReportAsync(reportType, reportName, folder.Id,  folder.Name, reportData);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException("Failed to upload report to server.");

        // 2) If both signatures exist, generate and upload PDF
        if (!string.IsNullOrEmpty(folder.Signature1) && !string.IsNullOrEmpty(folder.Signature2))
        {
            // Fetch signatures
            byte[] sig1 = await ApiService.GetImageAsByteArrayAsync($"https://ashwellmaintenance.host/{folder.Signature1}");
            byte[] sig2 = await ApiService.GetImageAsByteArrayAsync($"https://ashwellmaintenance.host/{folder.Signature2}");
            if (sig1 == null || sig2 == null)
                throw new Exception("Couldn't retrieve signatures.");

            // Generate PDF
            byte[] pdfData = await PdfCreation.BoilerHouseDataSheet(reportData, sig1, sig2);
            if (pdfData == null)
                throw new Exception("PDF generation returned null.");

            // Upload PDF
            HttpResponseMessage pdfResponse = await ApiService.UploadPdfToDropboxAsync(pdfData, folder.Name, reportName);
            if (!pdfResponse.IsSuccessStatusCode)
                throw new HttpRequestException("Failed to upload PDF to Dropbox.");
        }
    }

    /// <summary>
    /// Retries uploading all pending reports. Stops and retains the failed one; removes any successfully uploaded.
    /// </summary>
    public static async Task RetryPendingReportsAsync()
    {
        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            return;

        string folderPath = Path.Combine(FileSystem.AppDataDirectory, "PendingReports");
        string fullPath = Path.Combine(folderPath, "pending_reports.json");
        if (!File.Exists(fullPath))
            return;

        List<Report> pending;
        try
        {
            string existingJson = await File.ReadAllTextAsync(fullPath);
            pending = JsonSerializer.Deserialize<List<Report>>(existingJson) ?? new List<Report>();
        }
        catch
        {
            return;
        }

        // Load current folders for signature info
        HttpResponseMessage folderResp = await ApiService.GetAllFoldersAsync();
        var folders = new List<Folder>();
        if (folderResp.IsSuccessStatusCode)
        {
            string json = await folderResp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("data", out var arr))
            {
                folders = arr.EnumerateArray()
                             .Select(el => new Folder
                             {
                                 Id = el.GetProperty("folder_id").GetString(),
                                 Name = el.GetProperty("folder_name").GetString(),
                                 Signature1 = el.GetProperty("signature1").GetString(),
                                 Signature2 = el.GetProperty("signature2").GetString()
                             })
                             .ToList();
            }
        }

        var remaining = new List<Report>();
        foreach (var rpt in pending)
        {
            var folder = folders.FirstOrDefault(f => f.Id == rpt.FolderId);
            if (folder == null)
            {
                remaining.Add(rpt);
                break;
            }
            try
            {
                await UploadReportAsync(rpt.ReportType, rpt.ReportName, folder, rpt.ReportData);
            }
            catch
            {
                remaining.Add(rpt);
                break;
            }
        }

        try
        {
            if (remaining.Any())
            {
                string updated = JsonSerializer.Serialize(remaining, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(fullPath, updated);
            }
            else
            {
                File.Delete(fullPath);
            }
        }
        catch { }
    }

    /// <summary>
    /// Returns true if pending_reports.json exists **and** contains at least one Report.
    /// </summary>
    public static async Task<bool> HasPendingReportsAsync()
    {
        string folderPath = Path.Combine(FileSystem.AppDataDirectory, "PendingReports");
        string fullPath = Path.Combine(folderPath, "pending_reports.json");
        if (!File.Exists(fullPath))
            return false;

        try
        {
            string json = await File.ReadAllTextAsync(fullPath);
            var list = JsonSerializer.Deserialize<List<Report>>(json);
            return list != null && list.Count > 0;
        }
        catch
        {
            // If the file is corrupt, assume no valid pending reports
            return false;
        }
    }

    /// <summary>
    /// Saves a report locally for later retry.
    /// </summary>
    public static async Task SaveReportLocallyAsync(
        Enums.ReportType reportType,
        string reportName,
        Folder folder,
        Dictionary<string, string> reportData)
    {
        try
        {
            string localFolderPath = Path.Combine(FileSystem.AppDataDirectory, "PendingReports");
            if (!Directory.Exists(localFolderPath))
                Directory.CreateDirectory(localFolderPath);

            string fullPath = Path.Combine(localFolderPath, "pending_reports.json");
            List<Report> pending = new List<Report>();
            if (File.Exists(fullPath))
            {
                string existingJson = await File.ReadAllTextAsync(fullPath);
                pending = JsonSerializer.Deserialize<List<Report>>(existingJson) ?? new List<Report>();
            }

            pending.Add(new Report
            {
                ReportType = reportType,
                ReportName = reportName,
                FolderId = folder.Id,
                ReportData = reportData
            });

            string updated = JsonSerializer.Serialize(pending, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(fullPath, updated);
        }
        catch (Exception ex)
        {
            // optionally log the failure
        }
    }
}
