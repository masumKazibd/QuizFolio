using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizFolio.Data;
using QuizFolio.Models;
using QuizFolio.ViewModels;
using System.Security.Claims;
using System.Text.Json;

namespace QuizFolio.Controllers
{
    public class TemplateController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly AppDbContext _context;

        public TemplateController(SignInManager<Users> signInManager, UserManager<Users> userManager, AppDbContext context)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            _context = context;
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTemplate(TemplateCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var template = new Template
            {
                Title = model.Title,
                Description = model.Description,
                IsPublic = model.IsPublic,
                CreatorId = userId,
                Questions = model.Questions.Select(q => new Question
                {
                    Text = q.Text,
                    Type = q.Type,
                    OptionsJson = q.Type == QuestionType.Dropdown || q.Type == QuestionType.Radio
                        ? JsonSerializer.Serialize(q.Options)
                        : null,
                    IsRequired = q.IsRequired
                }).ToList()
            };

            _context.Templates.Add(template);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }
}
