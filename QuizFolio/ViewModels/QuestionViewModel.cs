using QuizFolio.Models;
using System.ComponentModel.DataAnnotations;

namespace QuizFolio.ViewModels
{
    public class QuestionViewModel
    {
        public QuestionViewModel()
        {
            this.Options = new List<QuestionOptionViewModel>();
        }
        public QuestionType QuestionType { get; set; }
        public string QuestionTitle { get; set; }
        public bool IsRequired { get; set; }
        public List<QuestionOptionViewModel> Options { get; set; } = new();
    }
}
