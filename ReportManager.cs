using Ashwell_Maintenance;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public static class ReportManager
{
    public static async Task UploadReportAsync(
        Enums.ReportType reportType,
        string reportName,
        Folder folder,
        Dictionary<string, string> reportData)
    {
        bool uploadSuccess = false;
        bool pdfUploadSuccess = false;

        try
        {
            HttpResponseMessage response = await ApiService.UploadReportAsync(reportType, reportName, folder.Id, reportData);
            uploadSuccess = response.IsSuccessStatusCode;

            if (!uploadSuccess)
                throw new HttpRequestException("Failed to upload report to server.");
        }
        catch (HttpRequestException)
        {
            await ShowError("No internet or server error. Saving report locally.");
        }
        catch (Exception ex)
        {
            await ShowError($"Unexpected error uploading report: {ex.Message}");
        }

        // PDF Upload logic if server upload succeeded and signatures exist
        if (uploadSuccess &&
            !string.IsNullOrEmpty(folder.Signature1) &&
            !string.IsNullOrEmpty(folder.Signature2))
        {
            try
            {
                byte[] signature1 = await ApiService.GetImageAsByteArrayAsync($"https://ashwellmaintenance.host/{folder.Signature1}");
                byte[] signature2 = await ApiService.GetImageAsByteArrayAsync($"https://ashwellmaintenance.host/{folder.Signature2}");

                if (signature1 == null || signature2 == null)
                    throw new Exception("Couldn't retrieve signatures.");

                byte[] pdfData = await PdfCreation.BoilerHouseDataSheet(reportData, signature1, signature2);

                if (pdfData != null)
                {
                    HttpResponseMessage pdfResponse = await ApiService.UploadPdfToDropboxAsync(pdfData, folder.Name, reportName);
                    pdfUploadSuccess = pdfResponse.IsSuccessStatusCode;

                    if (!pdfUploadSuccess)
                        throw new HttpRequestException("Failed to upload PDF to Dropbox.");
                }
            }
            catch (Exception ex)
            {
                await ShowError($"PDF upload failed: {ex.Message}");
            }
        }

        // Save locally if either main upload failed
        if (!uploadSuccess)
        {
            await SaveReportLocallyAsync(reportType, reportName, folder, reportData);
        }

        if (uploadSuccess)
        {
            await Application.Current.MainPage.DisplayAlert("Success", "Report successfully uploaded.", "OK");
        }
    }

    private static async Task SaveReportLocallyAsync(
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

            // Load existing pending reports if file exists
            List<Report> pendingReports = new List<Report>();
            if (File.Exists(fullPath))
            {
                string existingJson = await File.ReadAllTextAsync(fullPath);
                pendingReports = JsonSerializer.Deserialize<List<Report>>(existingJson) ?? new List<Report>();
            }

            // Add new report to the list
            pendingReports.Add(new Report
            {
                ReportType = reportType,
                ReportName = reportName,
                FolderId = folder.Id,
                ReportData = reportData
            });

            // Serialize updated list and write back
            string updatedJson = JsonSerializer.Serialize(pendingReports, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(fullPath, updatedJson);

            await Application.Current.MainPage.DisplayAlert("Saved", "No internet. Report saved locally.", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save report locally: {ex.Message}", "OK");
        }
    }


    private static Task ShowError(string message)
    {
        return Application.Current.MainPage.DisplayAlert("Error", message, "OK");
    }
}
