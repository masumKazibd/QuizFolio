using Microsoft.AspNetCore.Identity;

namespace QuizFolio.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }
        public string Designation { get; set; }
        public DateTime LoginTime { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsAdmin { get; set; }

        //Salesforce
        public string? SalesforceAccountId { get; set; }
        public string? SalesforceContactId { get; set; }
        public DateTime? SalesforceLastSync { get; set; }
        public bool SubscribeToNewsletter { get; set; }
        public bool ReceiveProductUpdates { get; set; }

        public ICollection<Template> Templates { get; set; }
        public ICollection<FormResponse> FormResponses { get; set; } = new List<FormResponse>();
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Like> Likes { get; set; }
    }
}
