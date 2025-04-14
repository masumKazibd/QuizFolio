using QuizFolio.Models;

namespace QuizFolio.ViewModels
{
    public class QuestionResponseViewModel
    {
        public int QuestionId { get; set; }
        public string Text { get; set; }
        public QuestionType Type { get; set; }
        public bool IsRequired { get; set; }
        public List<string>? Options { get; set; }
        public string? Answer { get; set; } // Bound to form inputs

    }
}
