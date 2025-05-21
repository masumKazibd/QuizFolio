using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace QuizFolio.Service.OneDrive
{
    public class OneDriveService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OneDriveService> _logger;
        private string _accessToken;
        private DateTime _tokenExpiration;

        public OneDriveService(
            IConfiguration configuration, 
            IHttpClientFactory httpClientFactory,
            ILogger<OneDriveService> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task SetAccessTokenAsync(string token)
        {
            _accessToken = token;
            _tokenExpiration = DateTime.UtcNow.AddHours(1); // Token typically expires in 1 hour
            _logger.LogInformation("Access token set successfully. Expires at: {Expiration}", _tokenExpiration);
        }

        public async Task<string> GetAccessTokenAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiration)
                {
                    return _accessToken;
                }

                // If we don't have a valid token, redirect to sign in
                throw new InvalidOperationException("No valid access token. Please sign in to OneDrive first.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get OneDrive access token");
                throw;
            }
        }

        private class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }
        }

        public async Task<bool> UploadFileAsync(string fileName, string fileContent)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var accessToken = await GetAccessTokenAsync();

                // First, ensure the SupportTickets folder exists
                var createFolderUrl = "https://graph.microsoft.com/v1.0/users/2023100010151@seu.edu.bd/drive/root:/SupportTickets:/children";
                var folderRequest = new HttpRequestMessage(HttpMethod.Post, createFolderUrl);
                folderRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                folderRequest.Content = new StringContent(
                    JsonSerializer.Serialize(new { name = "SupportTickets", folder = new { } }),
                    Encoding.UTF8,
                    "application/json");

                var folderResponse = await client.SendAsync(folderRequest);
                if (!folderResponse.IsSuccessStatusCode && folderResponse.StatusCode != System.Net.HttpStatusCode.Conflict)
                {
                    var errorContent = await folderResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to create SupportTickets folder. Status: {StatusCode}, Error: {Error}", 
                        folderResponse.StatusCode, errorContent);
                }

                // Now upload the file
                var uploadUrl = $"https://graph.microsoft.com/v1.0/users/2023100010151@seu.edu.bd/drive/root:/SupportTickets/{fileName}:/content";
                _logger.LogInformation("Uploading file to OneDrive: {Url}", uploadUrl);

                var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Content = new StringContent(fileContent, Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("OneDrive upload failed. Status: {StatusCode}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    return false;
                }

                _logger.LogInformation("File uploaded successfully to OneDrive: {FileName}", fileName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file to OneDrive: {FileName}", fileName);
                return false;
            }
        }

        public async Task<List<string>> ListUploadedFilesAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var accessToken = await GetAccessTokenAsync();

                var listUrl = "https://graph.microsoft.com/v1.0/users/2023100010151@seu.edu.bd/drive/root:/SupportTickets/children";
                //var listUrl = "https://graph.microsoft.com/v1.0/drives/root/root:/SupportTickets:/children";
                var request = new HttpRequestMessage(HttpMethod.Get, listUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to list files from OneDrive. Status: {StatusCode}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    return new List<string>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var files = JsonSerializer.Deserialize<OneDriveFileList>(content);
                return files?.Value?.Select(f => f.Name).ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list files from OneDrive");
                return new List<string>();
            }
        }

        public async Task<string> GetFileUrlAsync(string fileName)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var accessToken = await GetAccessTokenAsync();

                //var url = $"https://graph.microsoft.com/v1.0/drives/root/root:/SupportTickets/{fileName}:/content";
                var url = $"https://graph.microsoft.com/v1.0/users/2023100010151@seu.edu.bd/drive/root:/SupportTickets/{fileName}:/content";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to get file URL from OneDrive. Status: {StatusCode}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    return string.Empty;
                }

                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get file URL for: {FileName}", fileName);
                return string.Empty;
            }
        }

        private class OneDriveFileList
        {
            [JsonPropertyName("value")]
            public List<OneDriveFile> Value { get; set; }
        }

        private class OneDriveFile
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }
    }
}