using Microsoft.AspNetCore.Mvc;

namespace QuizFolio.Controllers
{
    public class FormController : Controller
    {
        public IActionResult CreateForm()
        {
            return View();
        }
    }
}
