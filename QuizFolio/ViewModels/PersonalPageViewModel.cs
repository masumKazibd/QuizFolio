using QuizFolio.Models;

namespace QuizFolio.ViewModels
{
    public class PersonalPageViewModel
    {

        //Salesforce
        public string SalesforceAccountId { get; set; }
        public string SalesforceContactId { get; set; }
        public IEnumerable<Template> Templates { get; set; }
        public IEnumerable<FormResponse> Forms { get; set; }
    }
}
