using System.ComponentModel.DataAnnotations;

namespace QuizFolio.ViewModels.Salesforce
{
    public class CrmIntegrationViewModel
    {
        [Required]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        // Add required Country field
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "State/Province")]
        public string State { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [Display(Name = "Street Address")]
        public string Address { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "Zip/Postal Code")]
        public string ZipCode { get; set; }

        [Display(Name = "Subscribe to newsletter")]
        public bool SubscribeToNewsletter { get; set; }

        [Display(Name = "Receive product updates")]
        public bool ReceiveProductUpdates { get; set; }
    }
}