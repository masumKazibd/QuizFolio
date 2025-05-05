namespace QuizFolio.ViewModels
{
    public class DashboardViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Designation { get; set; }
        public string UserRole { get; set; }
        public DateTime? LastLoginTIme { get; set; }
        public bool IsBlocked { get; set; }
        public DateTimeOffset? LockoutEnd {  get; set; }
    }
}
