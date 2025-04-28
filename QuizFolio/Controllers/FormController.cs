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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitForm(int TemplateId, List<QuestionResponseViewModel> Answers)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var answersToSave = Answers.Select(a => new
            {
                QuestionId = a.QuestionId,
                Options = a.Options
            }).ToList();

            var formResponse = new FormResponse
            {
                TemplateId = TemplateId,
                RespondentId = userId,
                AnswersJson = System.Text.Json.JsonSerializer.Serialize(answersToSave),
                SubmittedAt = DateTime.UtcNow
            };

            _context.FormResponses.Add(formResponse);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Form submitted successfully.";
            return RedirectToAction("AllTemplate", "Template");
        }
    }
}
