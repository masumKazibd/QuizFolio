using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuizFolio.Models;
using QuizFolio.Models.Salesforce;
using QuizFolio.Service.Salesforce;
using QuizFolio.Services;
using QuizFolio.Services.Salesforce;
using QuizFolio.ViewModels.Salesforce;
using static QuizFolio.Services.Salesforce.SalesforceService;

namespace QuizFolio.Controllers
{
    public class SalesforceController : Controller
    {
        private readonly ISalesforceService _salesforceService;
        private readonly ILogger<SalesforceController> _logger;
        private readonly UserManager<Users> _userManager;

        public SalesforceController(
            ISalesforceService salesforceService,
            ILogger<SalesforceController> logger,
            UserManager<Users> userManager)
        {
            _salesforceService = salesforceService;
            _logger = logger;
            _userManager = userManager;
        }
        public async Task<IActionResult> Limits()
        {
            try
            {
                await _salesforceService.Authenticate();
                var limits = await _salesforceService.GetLimits();
                return View(new LimitsViewModel { RawJson = limits });
            }
            catch (HttpRequestException ex)
            {
                TempData["WarningMessage"] = ex.Message;
                return RedirectToAction("Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> ConnectToCrm()
        {
            var user = await _userManager.GetUserAsync(User);
            var model = new CrmIntegrationViewModel
            {
                Email = user.Email,
                FirstName = user.FullName,
                LastName = user.FullName,
                Phone = user.PhoneNumber
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConnectToCrm(CrmIntegrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var accountResult = await _salesforceService.CreateAccount(
                    model.CompanyName,
                    model.Country,
                    model.State);

                // Create Contact linked to Account
                var contactResult = await _salesforceService.CreateContact(new SalesforceContactModel
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    AccountId = accountResult.Id,
                    Address = model.Address,
                    Country = model.Country,
                    City = model.City,
                    State = model.State,
                    ZipCode = model.ZipCode
                });

                // Update user record with Salesforce IDs
                var user = await _userManager.GetUserAsync(User);
                user.SalesforceAccountId = accountResult.Id;
                user.SalesforceContactId = contactResult.Id;
                await _userManager.UpdateAsync(user);

                TempData["Message"] = "Your account has been successfully connected to our CRM system!";
                return RedirectToAction("PersonalPage", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRM integration failed for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                TempData["WarningMessage"] = $"CRM integration failed for user {User.FindFirstValue(ClaimTypes.NameIdentifier)}"; ModelState.AddModelError("", "Failed to connect to CRM. Please try again later.");
                return View(model);
            }
        }


        [HttpGet]
        public IActionResult CreateAccount()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(CreateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _salesforceService.Authenticate();
                var result = await _salesforceService.CreateAccount(model.Name, model.Country, model.BillingState);
                return RedirectToAction("AccountCreated", new { message = "Account created successfully!" });
            }
            return View(model);
        }

        public IActionResult AccountCreated(string message)
        {
            ViewBag.Message = message;
            return View();
        }
    }
}
