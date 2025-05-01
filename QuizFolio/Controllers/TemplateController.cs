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
        [Authorize]
        public IActionResult EditTemplate(int id)
        {
            var template = _context.Templates
                .Include(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefault(t => t.Id == id);

            if (template == null)
            {
                return NotFound();
            }

            var model = new TemplateCreateViewModel
            {
                Title = template.Title,
                Description = template.Description,
                IsPublic = template.IsPublic,
                Questions = template.Questions.Select(q => new QuestionViewModel
                {
                    QuestionTitle = q.QuestionTitle,
                    QuestionType = q.QuestionType,
                    IsRequired = q.IsRequired,
                    Options = q.Options.Select(o => new QuestionOptionViewModel
                    {
                        Option = o.Option
                    }).ToList()
                }).ToList()
            };

            return View(model);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditTemplate(int id, TemplateCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Invalid form data.";
                return View(model);
            }

            var template = await _context.Templates
                .Include(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (template.CreatorId != userId)
            {
                TempData["WarningMessage"] = "You are not authorized to edit this template.";
                return RedirectToAction("AllTemplate");
            }

            // Update template fields
            template.Title = model.Title;
            template.Description = model.Description;
            template.IsPublic = model.IsPublic;

            // Remove existing questions and options
            _context.QuestionOptions.RemoveRange(template.Questions.SelectMany(q => q.Options));
            _context.Questions.RemoveRange(template.Questions);
            await _context.SaveChangesAsync();

            // Re-add updated questions and options
            template.Questions = model.Questions.Select(q => new Question
            {
                QuestionTitle = q.QuestionTitle,
                QuestionType = q.QuestionType,
                IsRequired = q.IsRequired,
                Options = q.Options?.Select(o => new QuestionOption
                {
                    Option = o.Option
                }).ToList() ?? new List<QuestionOption>()
            }).ToList();

            await _context.SaveChangesAsync();
            TempData["Message"] = "Template updated successfully.";
            return RedirectToAction("AllTemplate");
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

