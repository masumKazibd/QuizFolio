using System.ComponentModel.DataAnnotations;

namespace QuizFolio.ViewModels.Support
{
    public class SupportTicketViewModel
    {
        [Required(ErrorMessage = "Please provide a description of your issue")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Summary must be between 10 and 1000 characters")]
        public string Summary { get; set; }

        [Required(ErrorMessage = "Please select a priority level")]
        public string Priority { get; set; }

        public string Template { get; set; }
        public string ReturnUrl { get; set; }
    }
}
