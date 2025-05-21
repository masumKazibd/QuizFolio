using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizFolio.Data;
using QuizFolio.Models.Support;
using QuizFolio.Service.OneDrive;
using QuizFolio.ViewModels.Support;
using System.Text.Json;

namespace QuizFolio.Controllers
{
    public class SupportController : Controller
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly OneDriveService _oneDriveService;
        private readonly ILogger<SupportController> _logger;
        private readonly AppDbContext _context;

        public SupportController(
            OneDriveService oneDriveService, 
            TelemetryClient telemetryClient,
            ILogger<SupportController> logger, AppDbContext context)
        {
            _oneDriveService = oneDriveService;
            _telemetryClient = telemetryClient;
            _logger = logger;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> CreateTicket(string returnUrl)
        {
            try
            {
                _logger.LogInformation("Creating support ticket form. ReturnUrl: {ReturnUrl}", returnUrl);

                var model = new SupportTicketViewModel
                {
                    ReturnUrl = returnUrl ?? Url.Action("Index", "Home"),
                };

                // Async database call with NoTracking for read-only operation
                ViewBag.Templates = await _context.Templates
                    .AsNoTracking()
                    .ToListAsync();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateTicket GET action");
                TempData["WarningMessage"] = "An error occurred while loading the ticket form.";
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTicket(SupportTicketViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Log validation errors
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogWarning("Validation error: {ErrorMessage}", error.ErrorMessage);
                    }
                    
                    ViewBag.Templates = await _context.Templates
                        .AsNoTracking()
                        .ToListAsync();
                        
                    return View(model);
                }


                var ticket = new SupportTicket
                {
                    ReportedBy = User.Identity?.Name ?? "Anonymous",
                    Template = model.Template,
                    Link = model.ReturnUrl,
                    Priority = model.Priority,
                    Summary = model.Summary,
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Created ticket object: {@Ticket}", ticket);
                _telemetryClient.TrackEvent("SupportTicketSubmitted", new Dictionary<string, string>
                {
                    {"User", ticket.ReportedBy},
                    {"Priority", ticket.Priority},
                    {"Template", ticket.Template ?? "None"}
                });

                // Serialize with proper formatting for Power Automate
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var json = JsonSerializer.Serialize(ticket, options);
                var fileName = $"ticket_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString()[..8]}.json";

                _logger.LogInformation("Attempting to upload ticket to OneDrive: {FileName}", fileName);
                
                try
                {
                    var success = await _oneDriveService.UploadFileAsync(fileName, json);

                    if (success)
                    {
                        _logger.LogInformation("Support ticket uploaded successfully: {FileName}", fileName);
                        TempData["SuccessMessage"] = "Support ticket submitted successfully! We will review your request shortly.";
                        return Redirect(model.ReturnUrl ?? Url.Action("Index", "Home"));
                    }

                    _logger.LogError("Failed to upload support ticket to OneDrive: {FileName}", fileName);
                    ModelState.AddModelError("", "Failed to submit support ticket. Please try again later.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading ticket to OneDrive: {FileName}", fileName);
                    ModelState.AddModelError("", "An error occurred while submitting your ticket. Please try again later.");
                }
                
                // Repopulate templates for the view
                ViewBag.Templates = await _context.Templates
                    .AsNoTracking()
                    .ToListAsync();
                    
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateTicket POST action for user {User}", User.Identity?.Name);
                TempData["WarningMessage"] = "An unexpected error occurred while submitting your ticket. Please try again later.";
                
                // Repopulate templates for the view
                ViewBag.Templates = await _context.Templates
                    .AsNoTracking()
                    .ToListAsync();
                    
                return View(model);
            }
        }
    }
}
