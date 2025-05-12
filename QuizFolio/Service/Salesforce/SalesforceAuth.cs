using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace QuizFolio.Service.Salesforce
{
    public class SalesforceAuth
    {
        private readonly ILogger<SalesforceAuth> _logger;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _username;
        private readonly string _password;
        private readonly string _tokenEndpoint;

        public SalesforceAuth(
            ILogger<SalesforceAuth> logger,
            string clientId,
            string clientSecret,
            string username,
            string password,
            string loginUrl = "https://login.salesforce.com")
        {
            _logger = logger;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _username = username;
            _password = password;
            _tokenEndpoint = $"{loginUrl}/services/oauth2/token";
        }
        public async Task<AuthResponse> LoginAsync()
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret),
            new KeyValuePair<string, string>("username", _username),
            new KeyValuePair<string, string>("password", _password)
        })
            };

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _logger.LogInformation($"Authenticating with Salesforce at {_tokenEndpoint}");

            try
            {
                var response = await client.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogDebug($"Response status: {response.StatusCode}");
                _logger.LogDebug($"Response content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Authentication failed: {response.StatusCode} - {responseContent}");
                    throw new HttpRequestException($"Salesforce authentication failed. Status: {response.StatusCode}. Response: {responseContent}");
                }

                try
                {
                    return JsonConvert.DeserializeObject<AuthResponse>(responseContent) ??
                        throw new InvalidOperationException("Received null response from Salesforce");
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to parse Salesforce response");
                    throw new HttpRequestException("Invalid response format from Salesforce", jsonEx);
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error during Salesforce authentication");
                throw; // Re-throw the original HttpRequestException
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Salesforce authentication");
                throw new HttpRequestException("Unexpected error during Salesforce authentication", ex);
            }
        }
    }

    public class AuthResponse
    {
        public string access_token { get; set; }
        public string instance_url { get; set; }
        public string id { get; set; }
        public string token_type { get; set; }
        public string issued_at { get; set; }
        public string signature { get; set; }
    }
}