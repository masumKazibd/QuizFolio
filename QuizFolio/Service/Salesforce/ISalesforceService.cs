using QuizFolio.Models.Salesforce;
using static QuizFolio.Services.Salesforce.SalesforceService;

namespace QuizFolio.Service.Salesforce
{
    public interface ISalesforceService
    {
        Task Authenticate();
        Task<string> GetLimits();
        Task<AccountCreationResult> CreateAccount(string name,String country, string billingState);
        Task<ContactCreationResult> CreateContact(SalesforceContactModel contact);
    }
}