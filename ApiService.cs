using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Ashwell_Maintenance;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Http;
using System.Net;

public static class ApiService
{
    private static readonly string BaseApiUrl = "https://ashwellmaintenance.host";


    /// <summary>
    /// Logs in a user and saves their ID and admin status to CurrentUser.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="password">The password of the user.</param>
    /// <returns>A tuple containing a boolean indicating success or failure and a string for the error message if any.</returns>
    public static async Task<(bool isSuccess, string errorMessage)> LoginAsync(string username, string password)
    {
        using HttpClient client = new();
        var loginData = new { username, password };
        var jsonContent = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsync($"{BaseApiUrl}/login.php", jsonContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send login request: {ex.Message}");
            return (false, "Server error: Unable to send request.");
        }

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Login failed with status code: {response.StatusCode}");
            return (false, "Server error: Invalid response from server.");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

        if (data.TryGetProperty("error", out JsonElement error))
        {
            string errorMessage = error.GetString();
            if (errorMessage == "Invalid username or password")
            {
                return (false, "Invalid username or password.");
            }
            return (false, $"Server error: {errorMessage}");
        }

        string userId = data.GetProperty("user").GetProperty("id").GetString();
        string isAdminString = data.GetProperty("user").GetProperty("is_admin").GetString();
        bool isAdmin = isAdminString == "1";

        // Save the user data to CurrentUser
        CurrentUser.SetUser(userId, isAdmin);

        return (true, null);
    }

    /// <summary>
    /// Uploads a new folder to the server.
    /// </summary>
    /// <param name="folderName">The name of the folder to be created.</param>
    /// <returns>A HttpResponseMessage indicating the outcome of the API call.</returns>
    public static async Task<HttpResponseMessage> UploadFolderAsync(string folderName)
    {
        using HttpClient client = new();
        var folderData = new { folder_name = folderName };
        var jsonContent = new StringContent(JsonSerializer.Serialize(folderData), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync($"{BaseApiUrl}/create_folder.php", jsonContent);

        return response;
    }

    /// <summary>
    /// Renames an existing folder in both the database and Dropbox.
    /// </summary>
    /// <param name="folderId">The ID of the folder to be renamed.</param>
    /// <param name="newFolderName">The new name of the folder.</param>
    /// <returns>A HttpResponseMessage indicating the outcome of the API call.</returns>
    public static async Task<HttpResponseMessage> RenameFolderAsync(string folderId, string newFolderName)
    {
        // Get the old folder name
        string oldFolderName;
        try
        {
            oldFolderName = await GetFolderNameByIdAsync(folderId);
        }
        catch (Exception ex)
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent($"Error fetching old folder name from the server: {ex.Message}")
            };
        }

        // Rename in the database
        HttpResponseMessage dbResponse = await RenameFolderInDatabaseAsync(folderId, newFolderName);
        if (!dbResponse.IsSuccessStatusCode)
        {
            return dbResponse;
        }

        // Rename in Dropbox
        HttpResponseMessage dropboxResponse = await RenameFolderInDropboxAsync(oldFolderName, newFolderName);
        if (!dropboxResponse.IsSuccessStatusCode)
        {
            return dropboxResponse;
        }

        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("Folder renamed successfully in both the database and Dropbox.")
        };
    }

    /// <summary>
    /// Renames an existing folder in the database.
    /// </summary>
    /// <param name="folderId">The ID of the folder to be renamed.</param>
    /// <param name="newFolderName">The new name of the folder.</param>
    /// <returns>A HttpResponseMessage indicating the outcome of the API call.</returns>
    public static async Task<HttpResponseMessage> RenameFolderInDatabaseAsync(string folderId, string newFolderName)
    {
        using HttpClient client = new();
        var folderData = new { folder_id = folderId, new_folder_name = newFolderName };
        var jsonContent = new StringContent(JsonSerializer.Serialize(folderData), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync($"{BaseApiUrl}/rename_folder.php", jsonContent);
        return response;
    }

    /// <summary>
    /// Deletes a folder from both the server database and Dropbox.
    /// </summary>
    /// <param name="folderId">The ID of the folder to be deleted.</param>
    /// <returns>A HttpResponseMessage indicating the outcome of the API calls.</returns>
    public static async Task<HttpResponseMessage> DeleteFolderAsync(string folderId)
    {
        // Get the old folder name
        string folderName;
        try
        {
            folderName = await GetFolderNameByIdAsync(folderId);
        }
        catch (Exception ex)
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent($"Error fetching old folder name from the server: {ex.Message}")
            };
        }

        // Delete from the database
        HttpResponseMessage dbResponse = await DeleteFolderFromDatabaseAsync(folderId);
        if (!dbResponse.IsSuccessStatusCode)
        {
            return dbResponse;
        }

        // Delete from Dropbox
        HttpResponseMessage dropboxResponse = await DeleteFolderInDropboxAsync(folderName);
        if (!dropboxResponse.IsSuccessStatusCode)
        {
            return dropboxResponse;
        }

        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("Folder deleted successfully from both the database and Dropbox.")
        };
    }

    /// <summary>
    /// Deletes a folder from the server database.
    /// </summary>
    /// <param name="folderId">The ID of the folder to be deleted.</param>
    /// <returns>A HttpResponseMessage indicating the outcome of the API call.</returns>
    public static async Task<HttpResponseMessage> DeleteFolderFromDatabaseAsync(string folderId)
    {
        using HttpClient client = new();
        var folderData = new { folder_id = folderId };
        var jsonContent = new StringContent(JsonSerializer.Serialize(folderData), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync($"{BaseApiUrl}/delete_folder.php", jsonContent);
        return response;
    }


    /// <summary>
    /// Retrieves the old folder name by its ID.
    /// </summary>
    /// <param name="folderId">The ID of the folder.</param>
    /// <returns>The old folder name as a string.</returns>
    private static async Task<string> GetFolderNameByIdAsync(string folderId)
    {
        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync($"{BaseApiUrl}/get_folder_name.php?folder_id={folderId}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to retrieve folder name for ID: {folderId}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
        return data.GetProperty("folder_name").GetString();
    }

    /// <summary>
    /// Retrieves all folders with their names and IDs from the server.
    /// </summary>
    /// <returns>A HttpResponseMessage containing the list of folders.</returns>
    public static async Task<HttpResponseMessage> GetAllFoldersAsync()
    {
        using HttpClient client = new();

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
        using HttpClient client = new();

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
    /// <param name="customerSignatureBytes">The customer's signature image to be uploaded.</param>
    /// <param name="engineerSignatureBytes">The engineer's signature image to be uploaded.</param>
    /// <param name="folderId">The ID of the folder where the images should be saved.</param>
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
        content.Add(new StringContent(folderId), "folderId");

        HttpResponseMessage response = await client.PostAsync($"{BaseApiUrl}/upload_signatures.php", content);

        return response;
    }

    /// <summary>
    /// Downloads an image from the specified URL and returns its content as a byte array.
    /// </summary>
    /// <param name="imageUrl">The URL of the image to be downloaded.</param>
    /// <returns>A byte array representing the content of the downloaded image, or null if the download fails.</returns>
    public static async Task<byte[]> GetImageAsByteArrayAsync(string imageUrl)
    {
        using (HttpClient client = new())
        {
            try
            {
                // Download the image from the URL
                //byte[] imageData = await client.GetByteArrayAsync(imageUrl);

                // Send a GET request to the specified URL
                HttpResponseMessage response = await client.GetAsync(imageUrl);

                // Throw an exception if the response status code indicates failure
                response.EnsureSuccessStatusCode();

                // Read the response content as a byte array
                byte[] imageData = await response.Content.ReadAsByteArrayAsync();

                response.EnsureSuccessStatusCode();

                return imageData;
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., if the URL is invalid or the image couldn't be downloaded)
                await Shell.Current.DisplayAlert($"Error downloading image", ex.Message, "OK");
                var cekara = ex;
                return null;
            }
        }
    }


    private static readonly string _appKey = Secrets.AppKey; 
    private static readonly string _appSecret = Secrets.AppSecret;
    //private static readonly string _redirectUri = "http://localhost:12345/"; 
    private static string _accessToken;
    private static readonly string _uploadUrl = "https://content.dropboxapi.com/2/files/upload";
    private static readonly string _apiUrl = "https://api.dropboxapi.com/2";
    private static readonly string _refreshToken = Secrets.RefreshToken;
    /// <summary>
    /// Retrieves a new access token from Dropbox using the refresh token.
    /// </summary>
    /// <returns>The new access token as a string.</returns>
    /// <remarks>
    /// This method makes an asynchronous HTTP POST request to the Dropbox API
    /// to exchange the refresh token for a new access token.
    /// </remarks>
    public static async Task<string> GetNewAccessTokenAsync()
    {
        using (var client = new HttpClient())
        {
            try
            {
                var requestContent = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("refresh_token", _refreshToken),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", _appKey),
                new KeyValuePair<string, string>("client_secret", _appSecret)
            });

                var response = await client.PostAsync("https://api.dropbox.com/oauth2/token", requestContent);

                var jsonResponse = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    // Log detailed error
                    Console.WriteLine($"Error: {response.StatusCode}, Response: {jsonResponse}");
                    throw new HttpRequestException($"Request failed with status {response.StatusCode}");
                }

                var tokenData = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

                if (tokenData.ValueKind == JsonValueKind.Object && tokenData.TryGetProperty("access_token", out JsonElement accessTokenElement))
                {
                    return accessTokenElement.GetString();
                }
                else
                {
                    throw new InvalidOperationException("Failed to retrieve access token.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// Renames a folder in Dropbox.
    /// </summary>
    /// <param name="oldFolderName">The current name of the folder in Dropbox.</param>
    /// <param name="newFolderName">The new name of the folder in Dropbox.</param>
    /// <returns>A HttpResponseMessage indicating the outcome of the API call.</returns>
    public static async Task<HttpResponseMessage> RenameFolderInDropboxAsync(string oldFolderName, string newFolderName)
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            _accessToken = await GetNewAccessTokenAsync();
        }

        using HttpClient client = new();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        // Construct paths
        string oldPath = $"/{oldFolderName}";
        string newPath = $"/{newFolderName}";

        // Rename folder in Dropbox
        var renameData = new { from_path = oldPath, to_path = newPath };
        var renameContent = new StringContent(JsonSerializer.Serialize(renameData), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync($"{_apiUrl}/files/move_v2", renameContent);
        return response;
    }

    /// <summary>
    /// Deletes a folder in Dropbox.
    /// </summary>
    /// <param name="folderName">The name of the folder to be deleted.</param>
    /// <returns>A HttpResponseMessage indicating the outcome of the API call.</returns>
    public static async Task<HttpResponseMessage> DeleteFolderInDropboxAsync(string folderName)
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            _accessToken = await GetNewAccessTokenAsync();
        }

        using HttpClient client = new();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        // Construct the full path
        string path = $"/{folderName}";

        // Delete folder in Dropbox
        var deleteData = new { path = path };
        var deleteContent = new StringContent(JsonSerializer.Serialize(deleteData), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync($"{_apiUrl}/files/delete_v2", deleteContent);
        return response;
    }


    /// <summary>
    /// Uploads a PDF file to Dropbox.
    /// </summary>
    /// <param name="pdfData">The byte array representing the PDF file.</param>
    /// <param name="folderName">The name of the folder in Dropbox.</param>
    /// <param name="reportName">The name of the report.</param>
    /// <returns>A HttpResponseMessage indicating the outcome of the API call.</returns>
    public static async Task<HttpResponseMessage> UploadPdfToDropboxAsync(byte[] pdfData, string folderName, string reportName)
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            _accessToken = await GetNewAccessTokenAsync();
        }

        using HttpClient client = new();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        // Construct the full Dropbox path for the folder
        string folderPath = $"/{folderName}";

        try
        {
            // Check if the folder exists, and create it if it doesn't
            if (!await CheckFolderExistsDropboxAsync(client, folderPath))
            {
                await CreateFolderDropboxAsync(client, folderPath);
            }

            // Construct the full file path (adjust the file name as necessary)
            string filePath = $"{folderPath}/{reportName}.pdf";

            // Upload the file
            using HttpRequestMessage request = new(HttpMethod.Post, _uploadUrl);
            request.Headers.Add("Dropbox-API-Arg", $"{{\"path\": \"{filePath}\",\"mode\": \"add\",\"autorename\": true,\"mute\": false}}");
            request.Content = new ByteArrayContent(pdfData);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return await client.SendAsync(request);
        }
        catch (Exception ex)
        {
            // Return an error response message
            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                Content = new StringContent($"Error uploading PDF to Dropbox: {ex.Message}")
            };
        }
    }


    /// <summary>
    /// Checks if a folder exists in Dropbox.
    /// </summary>
    /// <param name="client">The HttpClient instance.</param>
    /// <param name="path">The path of the folder in Dropbox.</param>
    /// <returns>True if the folder exists, false otherwise.</returns>
    private static async Task<bool> CheckFolderExistsDropboxAsync(HttpClient client, string path)
    {
        var response = await client.PostAsJsonAsync($"{_apiUrl}/files/get_metadata", new { path });
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Creates a folder in Dropbox.
    /// </summary>
    /// <param name="client">The HttpClient instance.</param>
    /// <param name="path">The path of the folder to create in Dropbox.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task CreateFolderDropboxAsync(HttpClient client, string path)
    {
        var content = new StringContent(JsonSerializer.Serialize(new { path }), Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"{_apiUrl}/files/create_folder_v2", content);

        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create folder: {path}. Response: {responseContent}");
        }
    }


}
