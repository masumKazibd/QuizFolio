using QuizFolio.Models;

namespace QuizFolio.ViewModels
{
    public class QuestionViewModel
    {
        public QuestionType QuestionType { get; set; }
        public string QuestionTitle { get; set; }
        public bool IsRequired { get; set; }
        public List<string>? Options { get; set; } // For Dropdown/Radio
    }
}
