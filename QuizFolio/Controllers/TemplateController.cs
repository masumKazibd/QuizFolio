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
        //[Authorize]
        // GET: List all templates (public + user's private)
        public async Task<IActionResult> AllTemplate()
        {
            var templates = _context.Templates
                .Include(t => t.Creator)
                .Include(t => t.Questions)
                .Include(t => t.FormResponses)
                .ToList();

            return View(templates);
        }

        [Authorize]
        public IActionResult CreateTemplate()
        {
            return View(new TemplateCreateViewModel { Questions = new List<QuestionViewModel> { new QuestionViewModel() } });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTemplate(TemplateCreateViewModel model)
        {
            {
                if (ModelState.IsValid)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var template = new Template
                    {
                        Title = model.Title,
                        Description = model.Description,
                        IsPublic = model.IsPublic,
                        CreatorId = userId,
                        Questions = new List<Question>()
                    };

                    // Transform ViewModel Questions to model Questions
                    foreach (var questionViewModel in model.Questions)
                    {
                        var question = new Question
                        {
                            QuestionTitle = questionViewModel.QuestionTitle,
                            QuestionType = questionViewModel.QuestionType,
                            IsRequired = questionViewModel.IsRequired,
                            Options = new List<QuestionOption>()

                        };
                        // Map options if there are any
                        if (questionViewModel.Options != null && questionViewModel.Options.Any())
                        {
                            foreach (var optionViewModel in questionViewModel.Options)
                            {
                                var option = new QuestionOption
                                {
                                    Option = optionViewModel.Option
                                };
                                question.Options.Add(option);
                            }
                        }

                        template.Questions.Add(question);
                    }
                    _context.Templates.Add(template);
                    _context.SaveChanges();
                    TempData["Message"] = "Template created successfully.";
                    return RedirectToAction("AllTemplate");
                }
                else
                {
                    TempData["Message"] = "Invalid form";
                    return View(model);
                }
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //var isAdmin = User.IsInRole("Admin");

            //if (template.CreatorId != userId && !isAdmin)
            if (template.CreatorId != userId)
            {
                TempData["WarningMessage"] = "You are not the creator!";
                return RedirectToAction("AllTemplate");
                //return Forbid();
            }

            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Deleted Successfully!!!";

            return RedirectToAction("AllTemplate");
        }

    }
}

