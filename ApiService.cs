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
    /// Retrieves all reports for a specified folder ID from the server.
    /// </summary>
    /// <param name="folderId">The ID of the folder for which to retrieve reports.</param>
    /// <returns>A HttpResponseMessage containing the list of reports for the specified folder.</returns>
    public static async Task<HttpResponseMessage> GetReportsForFolderAsync(string folderId)
    {
        using HttpClient client = new HttpClient();

        // Append the folder_id parameter to the API endpoint
        string apiUrl = $"{BaseApiUrl}/get_reports_by_folder.php?folder_id={folderId}";

        HttpResponseMessage response = await client.GetAsync(apiUrl);

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
    public static async Task<HttpResponseMessage> UploadReportAsync(Enums.ReportType reportType, string reportName, string folderId, Dictionary<string, string> additionalReportData)
    {
        using HttpClient client = new();

        // Create a list to maintain the order of elements
        var postData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("report_type", reportType.ToString()),
            new KeyValuePair<string, string>("report_name", reportName),
            new KeyValuePair<string, string>("folder_id", folderId)
        };

        // Add additional report data to the list
        postData.AddRange(additionalReportData);

        var jsonContent = new StringContent(JsonSerializer.Serialize(postData.ToDictionary(x => x.Key, x => x.Value)), Encoding.UTF8, "application/json");

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
}
