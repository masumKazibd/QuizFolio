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
        public ICollection<Template> Templates { get; set; }
        public ICollection<Form> Forms { get; set; } = new List<Form>();

    }
}
