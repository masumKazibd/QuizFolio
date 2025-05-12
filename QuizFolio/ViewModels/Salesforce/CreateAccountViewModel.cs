using System.ComponentModel.DataAnnotations;

namespace QuizFolio.ViewModels.Salesforce
{
    public class CreateAccountViewModel
    {
        [Required]
        [Display(Name = "Account Name")]
        public string Name { get; set; }

        [Display(Name = "Billing State")]
        public string BillingState { get; set; }
        public string Country { get; set; }

    }
}