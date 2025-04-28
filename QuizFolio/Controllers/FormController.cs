using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizFolio.Data;
using QuizFolio.Models;
using QuizFolio.ViewModels;
using System.Security.Claims;

namespace QuizFolio.Controllers
{
    public class FormController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly AppDbContext _context;

        public FormController(SignInManager<Users> signInManager, UserManager<Users> userManager, AppDbContext context)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            _context = context;
        }
        [Authorize]
        public async Task<IActionResult> ViewForm(int id)
        {
            var template = await _context.Templates
                            .Include(t => t.Creator)
                            .Include(t => t.Questions)
                            .ThenInclude(q => q.Options)
                            .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
                return NotFound();
            return View(template);
        }
        [HttpPost]
        public async Task<IActionResult> SubmitForm(int TemplateId, List<QuestionResponseViewModel> Answers)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            List<FormResponse> responses = new List<FormResponse>();

            foreach (var answer in Answers)
            {
                var response = new FormResponse
                {
                    TemplateId = TemplateId,
                    RespondentId = userId,
                    QuestionId = answer.QuestionId,
                    AnswerText = answer.Answer,
                    AnswerOptionsJson = (answer.Options != null && answer.Options.Any())
                        ? System.Text.Json.JsonSerializer.Serialize(answer.Options)
                        : null,
                    SubmittedAt = DateTime.UtcNow
                };

                responses.Add(response);
            }

            _context.FormResponses.AddRange(responses);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Form submited successfully.";
            return RedirectToAction("AllTemplate", "Template");

        }

    }
}
