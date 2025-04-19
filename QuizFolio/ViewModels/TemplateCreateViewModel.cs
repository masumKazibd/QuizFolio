using QuizFolio.Models;
using System.ComponentModel.DataAnnotations;

namespace QuizFolio.ViewModels
{
    public class TemplateCreateViewModel
    {
        public TemplateCreateViewModel()
        {
            this.Questions = new List<QuestionViewModel>();
        }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }
        public List<QuestionViewModel> Questions { get; set; } = new();
    }
}
