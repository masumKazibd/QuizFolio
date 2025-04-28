using System.ComponentModel.DataAnnotations;

namespace QuizFolio.Models
{
    public class FormResponse
    {
        public int Id { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public int TemplateId { get; set; }
        public Template Template { get; set; }
        public string? RespondentId { get; set; }
        public Users? Respondent { get; set; }
        public string AnswersJson { get; set; } = string.Empty;
    }

}
