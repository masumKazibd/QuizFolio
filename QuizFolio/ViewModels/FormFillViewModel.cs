using QuizFolio.Models;

namespace QuizFolio.ViewModels
{
    public class FormFillViewModel
    {

        public int TemplateId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<QuestionResponseViewModel> Questions { get; set; }
        public bool EmailCopy { get; set; }
    }
}