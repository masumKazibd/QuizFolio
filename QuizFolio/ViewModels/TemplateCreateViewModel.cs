using QuizFolio.Models;
using System.ComponentModel.DataAnnotations;

namespace QuizFolio.ViewModels
{
    public class TemplateCreateViewModel
    {
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }
        public List<QuestionViewModel> Questions { get; set; } = new();
    }
}
