using QuizFolio.Models;

namespace QuizFolio.ViewModels
{
    public class FormAnswerDetailViewModel
    {
        public FormResponse Response { get; set; }
        public List<ParsedAnswerViewModel> ParsedAnswers { get; set; }
        public Dictionary<int, string> QuestionMap { get; set; }
    }
}
