using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Ashwell_Maintenance;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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
    /// <param name="customerSignature">The first image to be uploaded.</param>
    /// <param name="image2">The second image to be uploaded.</param>
    /// <param name="folderId">The ID of the folder where the images should be saved.</param>
    /// <param name="folderName">The name of the folder where the images should be saved.</param>
    /// <returns>A HttpResponseMessage indicating the outcome of the API call.</returns>
    public static async Task<HttpResponseMessage> UploadSignaturesAsync(byte[] customerSignatureBytes, byte[] engineerSignatureBytes, string folderId)
    {
        using HttpClient client = new();
        using MultipartFormDataContent content = new();

        // Convert byte arrays to streams for upload
        using var customerSignatureStream = new MemoryStream(customerSignatureBytes);
        using var engineerSignatureStream = new MemoryStream(engineerSignatureBytes);

        // Adding the two images with generated names
        content.Add(new StreamContent(customerSignatureStream), "signature1", $"customerSignature_{folderId}.jpg");
        content.Add(new StreamContent(engineerSignatureStream), "signature2", $"engineerSignature_{folderId}.jpg");

        HttpResponseMessage response = await client.PostAsync($"{BaseApiUrl}/upload_signatures.php", content);

        return response;
    }


    //private static readonly string _key = "kbyqio0zijuo2os"; 
    //private static readonly string _secret = "geruwzjd0qbbebe";
    private static readonly string _uploadUrl = "https://content.dropboxapi.com/2/files/upload";
    private static readonly string _accessToken = "sl.BqADOkI5uhZ-aN_HpM9IvOq50dtMQWijGZX3ppaN4op3KVqcVFg2mmwz0xZz9LJZrfO6RsUCCN54-p20AqBKGofVfVwRRFH5u4H6Q67tJfOlwHkFqt9qaXTxwshMdwPDbTFhJ-lcsQhR";
    private static readonly string _apiUrl = "https://api.dropboxapi.com/2";


    public static async Task<HttpResponseMessage> UploadPdfAsync(byte[] pdfData, string folderName)
    {
        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        // Construct the full Dropbox path for the folder
        string folderPath = $"/{folderName}";

        // Check if the folder exists, and create it if it doesn't
        if (!await CheckFolderExistsAsync(client, folderPath))
        {
            await CreateFolderAsync(client, folderPath);
        }

        // Construct the full file path (adjust the file name as necessary)
        string filePath = $"{folderPath}/file.pdf";

        // Upload the file
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _uploadUrl);
        request.Headers.Add("Dropbox-API-Arg", $"{{\"path\": \"{filePath}\",\"mode\": \"add\",\"autorename\": true,\"mute\": false}}");
        request.Content = new ByteArrayContent(pdfData);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        return await client.SendAsync(request);
    }

    private static async Task<bool> CheckFolderExistsAsync(HttpClient client, string path)
    {
        var response = await client.PostAsJsonAsync($"{_apiUrl}/files/get_metadata", new { path = path });
        return response.IsSuccessStatusCode;
    }

    private static async Task CreateFolderAsync(HttpClient client, string path)
    {
        var content = new StringContent(JsonSerializer.Serialize(new { path = path }), System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"{_apiUrl}/files/create_folder_v2", content);

        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create folder: {path}. Response: {responseContent}");
        }
    }

}
