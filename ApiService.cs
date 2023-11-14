using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Ashwell_Maintenance;

public static class ApiService
{
    private static readonly string BaseApiUrl = "https://ashwellmaintenance.host";

    /// <summary>
    /// Uploads a new folder to the server.
    /// </summary>
    /// <param name="folderName">The name of the folder to be created.</param>
    /// <returns>A HttpResponseMessage indicating the outcome of the API call.</returns>
    public static async Task<HttpResponseMessage> UploadFolderAsync(string folderName)
    {
        using HttpClient client = new HttpClient();
        var folderData = new { folder_name = folderName };
        var jsonContent = new StringContent(JsonSerializer.Serialize(folderData), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync($"{BaseApiUrl}/create_folder.php", jsonContent);

        return response;
    }
    /// <summary>
    /// Retrieves all folders with their names and IDs from the server.
    /// </summary>
    /// <returns>A HttpResponseMessage containing the list of folders.</returns>
    public static async Task<HttpResponseMessage> GetAllFoldersAsync()
    {
        using HttpClient client = new HttpClient();

        HttpResponseMessage response = await client.GetAsync($"{BaseApiUrl}/get_folders.php");

        return response;
    }

    /// <summary>
    /// Uploads a new report to the server.
    /// </summary>
    /// <param name="reportType">The type of the report.</param>
    /// <param name="reportName">The name of the report.</param>
    /// <param name="folderId">The ID of the folder where the report should be saved.</param>
    /// <param name="additionalReportData">A dictionary containing real values for the report.</param>
    /// <returns>A HttpResponseMessage indicating the outcome of the API call.</returns>
    public static async Task<HttpResponseMessage> UploadReportAsync(string reportType, string reportName, string folderId, Dictionary<string, string> additionalReportData)
    {
        using HttpClient client = new();
        // Combine provided data with additional report data
        var postData = new Dictionary<string, string>
        {
            {"report_type", reportType},
            {"report_name", reportName},
            {"folder_id", folderId}
        }.Concat(additionalReportData);

        var jsonContent = new StringContent(JsonSerializer.Serialize(postData), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync($"{BaseApiUrl}/upload_report.php", jsonContent);

        return response;
    }

    /// <summary>
    /// Uploads two images signatures to the server along with folder ID and folder name.
    /// </summary>
    /// <param name="image1">The first image to be uploaded.</param>
    /// <param name="image2">The second image to be uploaded.</param>
    /// <param name="folderId">The ID of the folder where the images should be saved.</param>
    /// <param name="folderName">The name of the folder where the images should be saved.</param>
    /// <returns>A HttpResponseMessage indicating the outcome of the API call.</returns>
    public static async Task<HttpResponseMessage> UploadImagesAsync(Stream image1, Stream image2, string folderId, string folderName)
    {
        using HttpClient client = new();
        using MultipartFormDataContent content = new()
        {
            // Adding the two images with generated names
            { new StreamContent(image1), "signature1", $"{folderName}_{folderId}.jpg" },
            { new StreamContent(image2), "signature2", $"{folderName}_{folderId}.jpg" },
        };

        HttpResponseMessage response = await client.PostAsync($"{BaseApiUrl}/upload_signatures.php", content);

        return response;
    }

    internal static Task<HttpResponseMessage> UploadReportAsync(Enums.ReportType serviceRecord, string reportName, string folderId, Dictionary<string, string> report)
    {
        throw new NotImplementedException();
    }

    internal static Task<HttpResponseMessage> GetReportsForFolderAsync(string folderId)
    {
        throw new NotImplementedException();
    }
}
