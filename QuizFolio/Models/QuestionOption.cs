namespace QuizFolio.Models
{
    public class QuestionOption
    {
        public int Id { get; set; }
        public string Option { get; set; }

        // Foreign key
        public int QuestionId { get; set; }
        public Question Question { get; set; }
    }
}
