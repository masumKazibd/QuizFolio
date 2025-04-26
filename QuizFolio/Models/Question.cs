namespace QuizFolio.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string QuestionTitle { get; set; }
        public QuestionType QuestionType { get; set; }
        public bool IsRequired { get; set; }
        public ICollection<QuestionOption> Options { get; set; }

        // Foreign key
        public int TemplateId { get; set; }
        public Template Template { get; set; }
    }
}
