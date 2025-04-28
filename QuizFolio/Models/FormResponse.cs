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
        public int QuestionId { get; set; }

        public string? AnswerText { get; set; } // For single answer (Text, Radio, Dropdown, Email)

        public string? AnswerOptionsJson { get; set; } // For multiple options (Checkbox)

    }
}
