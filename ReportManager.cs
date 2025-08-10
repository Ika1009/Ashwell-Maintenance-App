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
        // If the folder.Id is null, this will create (or queue) it and return a valid Id.
        folder.Id = await FolderManager.EnsureFolderIdAsync(folder);

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
            byte[] pdfData = reportType switch
            {
                Enums.ReportType.BoilerHouseDataSheet => await PdfCreation.BoilerHouseDataSheet(reportData, sig1, sig2),
                Enums.ReportType.ConformityCheck => await PdfCreation.ConformityCheck(reportData, sig1, sig2),
                Enums.ReportType.ConstructionDesignManagement => await PdfCreation.ConstructionDesignManagement(reportData, sig1, sig2),
                Enums.ReportType.EngineersReport => await PdfCreation.EngineersReport(reportData, sig1, sig2),
                Enums.ReportType.GasRiskAssessment => await PdfCreation.GasRiskAssessment(reportData, sig1, sig2),
                Enums.ReportType.OneA => await PdfCreation.OneA(reportData, sig1, sig2),
                Enums.ReportType.OneB => await PdfCreation.OneB(reportData, sig1, sig2),
                Enums.ReportType.One => await PdfCreation.One(reportData, sig1, sig2),
                Enums.ReportType.PressurisationUnitReport => await PdfCreation.PressurisationReport(reportData, sig1, sig2),
                Enums.ReportType.ServiceRecord => await PdfCreation.ServiceRecord(reportData, sig1, sig2),
                _ => null
            };
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
        // 1) Only run when online
        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            return;

        string folderPath = Path.Combine(FileSystem.AppDataDirectory, "PendingReports");
        string fullPath = Path.Combine(folderPath, "pending_reports.json");
        if (!File.Exists(fullPath))
            return;

        // 2) Load all pending reports (each has FolderId and FolderName)
        List<Report> pending;
        try
        {
            string existingJson = await File.ReadAllTextAsync(fullPath);
            pending = JsonSerializer.Deserialize<List<Report>>(existingJson)
                      ?? new List<Report>();
        }
        catch
        {
            return; // corrupt file → give up
        }

        var stillPending = new List<Report>();

        // 3) Process each pending report in order
        foreach (var rpt in pending)
        {
            // a) Build a folder stub with Name (and possibly null Id)
            var folder = new Folder
            {
                Id = rpt.FolderId,
                Name = rpt.FolderName
            };

            // b) Ensure the folder exists (create if Id==null)
            try
            {
                folder.Id = await FolderManager.EnsureFolderIdAsync(folder);
            }
            catch
            {
                // failed to create or retrieve → stop here
                stillPending.Add(rpt);
                break;
            }

            // c) Now upload report (data + PDF)
            try
            {
                await UploadReportAsync(rpt.ReportType, rpt.ReportName, folder, rpt.ReportData);

            }
            catch
            {
                // upload failed → stop here
                stillPending.Add(rpt);
                break;
            }
        }

        // 4) Rewrite or delete the pending file
        try
        {
            if (stillPending.Any())
            {
                string updated = JsonSerializer.Serialize(stillPending,
                                       new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(fullPath, updated);
            }
            else
            {
                File.Delete(fullPath);
            }
        }
        catch
        {
            // ignore I/O errors
        }
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
                FolderName = folder.Name,
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
