using QuizFolio.Models;

namespace QuizFolio.ViewModels
{
    public class PersonalPageViewModel
    {
        public IEnumerable<Template> Templates { get; set; }
        public IEnumerable<FormResponse> Forms { get; set; }
    }
}
