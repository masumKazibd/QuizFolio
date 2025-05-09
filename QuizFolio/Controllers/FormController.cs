﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
            if (Answers == null || !Answers.Any())
            {
                TempData["Message"] = "You must answer at least one question!";
                return RedirectToAction("ViewForm", "Form", new { id = TemplateId });
            }

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
        public async Task<IActionResult> ViewRespondentList(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var template = await _context.Templates
                    .Include(t => t.Creator)
                    .Include(t => t.FormResponses)
                        .ThenInclude(r => r.Respondent)
                    .FirstOrDefaultAsync(t => t.Id == id);
                return View(template);
            }
            else
            {
                TempData["WarningMessage"] = "You are not logged in!!";
            }
            return RedirectToAction("AllTemplate", "Template");
        }
        public async Task<IActionResult> ViewAnswer(int id)
        {

            var response = await _context.FormResponses
                .Include(f => f.Template)
                    .ThenInclude(t => t.Creator)
                .Include(f => f.Respondent)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (response == null)
                return NotFound();

            var parsedAnswers = JsonConvert.DeserializeObject<List<ParsedAnswerViewModel>>(response.AnswersJson);

            var questionMap = await _context.Questions
                .Where(q => q.TemplateId == response.TemplateId)
                .ToDictionaryAsync(q => q.Id, q => q.QuestionTitle);

            var vm = new FormAnswerDetailViewModel
            {
                Response = response,
                ParsedAnswers = parsedAnswers,
                QuestionMap = questionMap
            };

            return View(vm);
        }
    }
}
