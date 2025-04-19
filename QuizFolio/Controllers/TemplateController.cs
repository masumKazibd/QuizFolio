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
        // GET: List all templates (public + user's private)
        public async Task<IActionResult> AllTemplate()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var templates = await _context.Templates
                .Where(t => t.IsPublic || t.CreatorId == userId)
                .Include(t => t.Questions)
                .ToListAsync();

            return View(templates);
        }

        // GET: Create template form
        public IActionResult CreateTemplate()
        {
            return View(new TemplateCreateViewModel { Questions = new List<QuestionViewModel> { new QuestionViewModel() } });
        }

        // POST: Save new template
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
                    QuestionTitle = q.QuestionTitle,
                    QuestionType = q.QuestionType,
                    OptionsJson = q.QuestionType == QuestionType.Dropdown || q.QuestionType == QuestionType.Radio
                        ? JsonSerializer.Serialize(q.Options)
                        : null,
                    IsRequired = q.IsRequired
                }).ToList()
            };

            _context.Templates.Add(template);
            await _context.SaveChangesAsync();

            return RedirectToAction("AllTemplate");
        }

        // GET: Edit template (only creator/admin)
        public async Task<IActionResult> EditTemplate(int id)
        {
            var template = await _context.Templates
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (template.CreatorId != userId && !isAdmin)
                return Forbid();

            var model = new TemplateCreateViewModel
            {
                Title = template.Title,
                Description = template.Description,
                IsPublic = template.IsPublic,
                Questions = template.Questions.Select(q => new QuestionViewModel
                {
                    QuestionTitle = q.QuestionTitle,
                    QuestionType = q.QuestionType,
                    Options = q.QuestionType == QuestionType.Dropdown || q.QuestionType == QuestionType.Radio
                        ? JsonSerializer.Deserialize<List<string>>(q.OptionsJson)
                        : null,
                    IsRequired = q.IsRequired
                }).ToList()
            };

            return View(model);
        }

        // POST: Update template
        [HttpPost]
        public async Task<IActionResult> EditTemplate(int id, TemplateCreateViewModel model)
        {
            var template = await _context.Templates
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (template.CreatorId != userId && !isAdmin)
                return Forbid();

            template.Title = model.Title;
            template.Description = model.Description;
            template.IsPublic = model.IsPublic;

            // Clear existing questions
            _context.Questions.RemoveRange(template.Questions);

            // Add updated questions
            template.Questions = model.Questions.Select(q => new Question
            {
                QuestionTitle = q.QuestionTitle,
                QuestionType = q.QuestionType,
                OptionsJson = q.QuestionType == QuestionType.Dropdown || q.QuestionType == QuestionType.Radio
                    ? JsonSerializer.Serialize(q.Options)
                    : null,
                IsRequired = q.IsRequired,
                TemplateId = template.Id
            }).ToList();

            await _context.SaveChangesAsync();
            return RedirectToAction("AllTemplate");
        }

        // POST: Delete template
        [HttpPost]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (template.CreatorId != userId && !isAdmin)
                return Forbid();

            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();

            return RedirectToAction("AllTemplate");
        }
    }
}

