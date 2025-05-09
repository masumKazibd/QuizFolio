using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuizFolio.Data;
using QuizFolio.Models;
using QuizFolio.ViewModels;
using System.Diagnostics.Eventing.Reader;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

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
        public IActionResult Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return View(new List<Template>());
            }

            var results = _context.Templates
                .Where(t => t.Title.Contains(searchTerm) ||
                            t.Description.Contains(searchTerm) ||
                            t.Questions.Any(q => q.QuestionTitle.Contains(searchTerm)) ||
                            t.Comments.Any(c => c.Content.Contains(searchTerm)))
                .ToList();

            return View(results);
        }
        public async Task<IActionResult> AllTemplate()
        {
            ViewBag.Topics = _context.Topics.ToList();
            var templates = _context.Templates
                .Include(t => t.Topic)
                .Include(t => t.Creator)
                .Include(t => t.Questions)
                .Include(t => t.FormResponses)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .Include(t => t.Likes)
                    .ThenInclude(l => l.User)
                .ToList();

            return View(templates);
        }
        [HttpPost]
        public async Task<IActionResult> LikeTemplate(int id)
        {
            if (User.Identity.IsAuthenticated)
            {

                var template = await _context.Templates.FindAsync(id);
                if (template == null)
                {
                    TempData["WarningMessage"] = "Template not found.";
                    return RedirectToAction("AllTemplate");

                }
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var existingLike = await _context.Likes
                    .FirstOrDefaultAsync(l => l.TemplateId == id && l.UserId == userId);
                if (existingLike != null)
                {
                    _context.Likes.Remove(existingLike);
                    await _context.SaveChangesAsync();
                    TempData["WarningMessage"] = "Like removed.";
                }
                else
                {
                    var like = new Like
                    {
                        TemplateId = id,
                        UserId = userId
                    };
                    _context.Likes.Add(like);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Like added successfully.";
                }
            }
            else
            {
                TempData["WarningMessage"] = "Please login first";
            }
            return RedirectToAction("AllTemplate");

        }
        [HttpPost]
        public async Task<IActionResult> AddComment(int id, string content)
        {
            if (User.Identity.IsAuthenticated)
            {
                var template = await _context.Templates.FindAsync(id);
                if (template == null)
                {
                    TempData["WarningMessage"] = "Template not found.";
                    return RedirectToAction("AllTemplate");
                }
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var comment = new Comment
                {
                    Content = content,
                    TemplateId = id,
                    UserId = userId
                };
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Comment added successfully.";
            }
            else
            {
                TempData["WarningMessage"] = "You are not logged in.";
            }
            return RedirectToAction("AllTemplate");
        }

        public IActionResult CreateTemplate()
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.Topics = new SelectList(_context.Topics, "Id", "TopicName");
                return View(new TemplateCreateViewModel { Questions = new List<QuestionViewModel> { new QuestionViewModel() } });
            }
            else
            {
                TempData["WarningMessage"] = "You are not logged in.";
            }
            return RedirectToAction("AllTemplate");
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
                        TopicId = model.TopicId,
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

        public async Task<IActionResult> EditTemplate(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var template = _context.Templates
                    .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                    .FirstOrDefault(t => t.Id == id);

                if (template == null)
                {
                    return NotFound();
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = await userManager.IsInRoleAsync(await userManager.GetUserAsync(User), "Admin");

                if (template.CreatorId != userId && !isAdmin)
                {
                    TempData["WarningMessage"] = "You do not have permission to edit this template.";
                    return RedirectToAction("AllTemplate", "Template");
                }

                var model = new TemplateCreateViewModel
                {
                    Title = template.Title,
                    Description = template.Description,
                    IsPublic = template.IsPublic,
                    TopicId = template.TopicId,
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
                ViewBag.Topics = new SelectList(_context.Topics, "Id", "TopicName", model.TopicId);
                return View("EditTemplate", model);
            }
            else
            {
                TempData["WarningMessage"] = "You are not logged in";
            }
            return RedirectToAction("AllTemplate", "Template");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditTemplate(int id, TemplateCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Invalid form data.";
                return View("EditTemplate", model);
            }

            var template = await _context.Templates
                .Include(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
            {
                TempData["WarningMessage"] = "Template not found.";
                return RedirectToAction("AllTemplate");
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
            template.TopicId = model.TopicId;
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

        [HttpPost]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            if (User.Identity.IsAuthenticated)
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
            else
            {
                TempData["WarningMessage"] = "You are not logged in";
            }
            return RedirectToAction("AllTemplate");
        }

    }
}

