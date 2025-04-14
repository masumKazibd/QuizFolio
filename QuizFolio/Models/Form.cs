namespace QuizFolio.Models
{
    public class Form
    {
        public int Id { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public int TemplateId { get; set; }
        public Template Template { get; set; }
        public string? RespondentId { get; set; } // Null if guest
        public Users? Respondent { get; set; }
        public ICollection<Answer> Answers { get; set; }
    }
}
