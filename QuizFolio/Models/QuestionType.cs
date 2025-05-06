using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace QuizFolio.Models
{
    public enum QuestionType
    {
        [Display(Name = "Single Line Text")]
        Text,
        [Display(Name = "Multi Line Text")]
        TextArea,
        Dropdown,
        Radio,
        Checkbox,
        Email
    }
}