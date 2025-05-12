using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QuizFolio.Models.Salesforce;
using QuizFolio.Service.Salesforce;

namespace QuizFolio.Services.Salesforce
{
    public class SalesforceService : ISalesforceService, IDisposable
    {
        private readonly ILogger<SalesforceService> _logger;
        private readonly HttpClient _httpClient;
        private readonly SalesforceAuth _authService;
        private AuthResponse _authResponse;
        private bool _disposed;

        public SalesforceService(
            ILogger<SalesforceService> logger,
            IHttpClientFactory httpClientFactory,
            SalesforceAuth authService)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _authService = authService;
        }

        public async Task Authenticate()
        {
            _authResponse = await _authService.LoginAsync();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _authResponse.access_token);
            _httpClient.BaseAddress = new Uri(_authResponse.instance_url);
        }

        public async Task<string> GetLimits()
        {
            try
            {
                var response = await _httpClient.GetAsync("/services/data/v56.0/limits");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Salesforce limits");
                throw;
            }
        }

        public async Task<AccountCreationResult> CreateAccount(string name, string country, string billingState)
        {
            await EnsureAuthenticated();

            var accountData = new
            {
                Name = name,
                BillingCountry = country,
                BillingState = billingState
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(accountData),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                "/services/data/v56.0/sobjects/Account/",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Account creation failed: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AccountCreationResult>(responseContent);
        }
        public async Task<ContactCreationResult> CreateContact(SalesforceContactModel contact)
        {
            try
            {
                await EnsureAuthenticated();

                var contactData = new
                {
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    Email = contact.Email,
                    Phone = contact.Phone,
                    AccountId = contact.AccountId, // Link to existing account
                    MailingStreet = contact.Address,
                    MailingCity = contact.City,
                    MailingState = contact.State,
                    MailingPostalCode = contact.ZipCode,
                    Description = $"Created from website on {DateTime.UtcNow:yyyy-MM-dd}"
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(contactData),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(
                    "/services/data/v56.0/sobjects/Contact/",
                    content);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ContactCreationResult>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Contact creation failed for {Email}", contact.Email);
                throw;
            }
        }

        public class ContactCreationResult
        {
            public string Id { get; set; }
            public bool Success { get; set; }
            public List<SalesforceError> Errors { get; set; }
        }
        public class AccountCreationResult
        {
            public string Id { get; set; }
            public bool Success { get; set; }
            public List<SalesforceError> Errors { get; set; }
        }

        public class SalesforceError
        {
            public string StatusCode { get; set; }
            public string Message { get; set; }
            public string[] Fields { get; set; }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                }
                _disposed = true;
            }
        }
        private async Task EnsureAuthenticated()
        {
            // Check if we have no token or if token is expired (Salesforce tokens typically last 1 hour)
            if (_authResponse == null || IsTokenExpired())
            {
                await Authenticate();
            }
        }

        private bool IsTokenExpired()
        {
            if (_authResponse == null || string.IsNullOrEmpty(_authResponse.issued_at))
                return true;

            try
            {
                var issuedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(_authResponse.issued_at));
                return DateTimeOffset.UtcNow >= issuedAt.AddHours(1); // Salesforce tokens typically expire in 1 hour
            }
            catch
            {
                return true; // If we can't parse the timestamp, assume expired
            }
        }
    }
}