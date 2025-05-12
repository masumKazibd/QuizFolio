namespace QuizFolio.Service.Salesforce
{
    public class SalesforceSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string LoginUrl { get; set; } = "https://login.salesforce.com";
    }
}
