using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

public static class OfflineReportManager
{
    private static readonly HttpClient HttpClient = new();

    public static async Task<bool> UploadReportAsync(string reportName, string folderId, Dictionary<string, string> reportData)
    {
        bool reportUploaded = false;
        try
        {
            HttpResponseMessage response = await ApiService.UploadReportAsync("BoilerHouseDataSheet", reportName, folderId, reportData);
            if (!response.IsSuccessStatusCode)
            {
                await OfflineReportManager.SaveReportAsync("BoilerHouseDataSheet", reportName, reportData);
                return false;
            }
            reportUploaded = true;
        }
        catch
        {
            await OfflineReportManager.SaveReportAsync("BoilerHouseDataSheet", reportName, reportData);
            return false;
        }

        // Fetch folder details to check signatures
        Folder folder = await ApiService.GetFolderByIdAsync(folderId);
        if (string.IsNullOrEmpty(folder?.Signature1) || string.IsNullOrEmpty(folder?.Signature2))
            return reportUploaded;

        try
        {
            byte[] signature1 = await ApiService.GetImageAsByteArrayAsync($"https://ashwellmaintenance.host/{folder.Signature1}");
            byte[] signature2 = await ApiService.GetImageAsByteArrayAsync($"https://ashwellmaintenance.host/{folder.Signature2}");
            if (signature1 == null || signature2 == null)
                throw new Exception("Couldn't retrieve signatures");

            byte[] pdfData = await PdfCreation.BoilerHouseDataSheet(reportData, signature1, signature2);
            if (pdfData != null)
            {
                HttpResponseMessage signatureResponse = await ApiService.UploadPdfToDropboxAsync(pdfData, folder.Name, reportName);
                if (!signatureResponse.IsSuccessStatusCode)
                {
                    await ApiService.DeleteReportAsync(reportName, folderId);
                    await OfflineReportManager.SaveReportAsync("BoilerHouseDataSheet", reportName, reportData);
                    return false;
                }
            }
        }
        catch
        {
            await ApiService.DeleteReportAsync(reportName, folderId);
            await OfflineReportManager.SaveReportAsync("BoilerHouseDataSheet", reportName, reportData);
            return false;
        }

        return true;
    }

    public static async Task RetryPendingUploads()
    {
        await OfflineReportManager.RetryPendingUploadsAsync();
    }
}
