namespace QuizFolio.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string QuestionTitle { get; set; }
        public QuestionType QuestionType { get; set; } // Enum (Text, Dropdown, etc.)
        public bool IsRequired { get; set; }
        public string? OptionsJson { get; set; } // Serialized list for Dropdown/Radio

        // Foreign key
        public int TemplateId { get; set; }
        public Template Template { get; set; }
    }
}
