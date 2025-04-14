using QuizFolio.Models;

namespace QuizFolio.ViewModels
{
    public class QuestionViewModel
    {
        public QuestionType Type { get; set; }
        public string Text { get; set; }
        public bool IsRequired { get; set; }
        public List<string>? Options { get; set; } // For Dropdown/Radio
    }
}
