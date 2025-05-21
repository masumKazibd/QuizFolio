namespace QuizFolio.Models.Support
{
    public class SupportTicket
    {
        public string ReportedBy { get; set; }
        public string Template { get; set; }
        public string Link { get; set; }
        public string Priority { get; set; }
        public string Summary { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
