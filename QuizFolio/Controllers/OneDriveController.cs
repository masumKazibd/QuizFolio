using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using QuizFolio.Service.OneDrive;

namespace QuizFolio.Controllers
{
    public class OneDriveController : Controller
    {
        private readonly OneDriveService _oneDriveService;
        private readonly ILogger<OneDriveController> _logger;

        public OneDriveController(
            OneDriveService oneDriveService,
            ILogger<OneDriveController> logger)
        {
            _oneDriveService = oneDriveService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("Callback"),
                Items =
                {
                    { "scope", "https://graph.microsoft.com/Files.ReadWrite" }
                }
            };

            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);
                if (!result.Succeeded)
                {
                    _logger.LogError("Authentication failed: {Error}", result.Failure?.Message);
                    return RedirectToAction("Error", "Home");
                }

                var token = await HttpContext.GetTokenAsync("access_token");
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("Failed to get access token from authentication result");
                    return RedirectToAction("Error", "Home");
                }

                // Store the token in the OneDrive service
                await _oneDriveService.SetAccessTokenAsync(token);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OneDrive callback");
                return RedirectToAction("Error", "Home");
            }
        }
    }
} 