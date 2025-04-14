namespace QuizFolio.Models
{
    public class Answer
    {
        public int Id { get; set; }
        public string Value { get; set; } // JSON for multiple-choice answers

        // Foreign keys
        public int FormId { get; set; }
        public Form Form { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
    }
}
