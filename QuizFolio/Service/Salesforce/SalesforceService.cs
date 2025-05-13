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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SalesforceAuth _authService;
        private AuthResponse _authResponse;
        private bool _disposed;

        public SalesforceService(
            ILogger<SalesforceService> logger,
            IHttpClientFactory httpClientFactory,
            SalesforceAuth authService)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _authService = authService;
        }

        public async Task Authenticate()
        {
            _authResponse = await _authService.LoginAsync();
        }

        public async Task<string> GetLimits()
        {
            try
            {
                using var client = CreateAuthenticatedClient();
                var response = await client.GetAsync("/services/data/v56.0/limits");
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

            using var client = CreateAuthenticatedClient();

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

            var response = await client.PostAsync(
                "/services/data/v56.0/sobjects/Account/",
                content);

            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<AccountCreationResult>(
                await response.Content.ReadAsStringAsync());
        }

        public async Task<ContactCreationResult> CreateContact(SalesforceContactModel contact)
        {
            try
            {
                await EnsureAuthenticated();
                using var client = CreateAuthenticatedClient();

                var contactData = new
                {
                    //FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    Email = contact.Email,
                    //Phone = contact.Phone,
                    //AccountId = contact.AccountId,
                    //MailingStreet = contact.Address,
                    //MailingCountry = contact.Country,
                    //MailingCity = contact.City,
                    //MailingState = contact.State,
                    //MailingPostalCode = contact.ZipCode,
                    //Description = $"Created from website on {DateTime.UtcNow:yyyy-MM-dd}"
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(contactData),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(
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
            if (!_disposed)
            {
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private async Task EnsureAuthenticated()
        {
            if (_authResponse == null || IsTokenExpired())
            {
                await Authenticate();
            }
        }

        private HttpClient CreateAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _authResponse.access_token);
            client.BaseAddress = new Uri(_authResponse.instance_url);
            return client;
        }

        private bool IsTokenExpired()
        {
            if (_authResponse == null || string.IsNullOrEmpty(_authResponse.issued_at))
                return true;

            try
            {
                var issuedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(_authResponse.issued_at));
                return DateTimeOffset.UtcNow >= issuedAt.AddHours(1);
            }
            catch
            {
                return true;
            }
        }
    }
}