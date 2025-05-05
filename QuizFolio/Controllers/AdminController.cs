using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizFolio.Models;
using QuizFolio.ViewModels;

namespace QuizFolio.Controllers;

public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly UserManager<Users> _userManager;
    private readonly SignInManager<Users> _signInManager;

    public AdminController(ILogger<AdminController> logger, UserManager<Users> userManager, SignInManager<Users> signInManager)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
    }
    public async Task<IActionResult> Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            var users = _userManager.Users.ToList();
            var model = new List<DashboardViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new DashboardViewModel
                {
                    Name = user.FullName,
                    Designation = user.Designation,
                    Email = user.Email,
                    LastLoginTIme = user.LoginTime,
                    LockoutEnd = user.LockoutEnd,
                    IsBlocked = user.IsBlocked,
                    UserRole = roles.FirstOrDefault() ?? "User"
                });
            }

            return View(model.OrderByDescending(vm => vm.LastLoginTIme).ToList());
        }

        TempData["WarningMessage"] = "You are not logged in";
        return RedirectToAction("AllTemplate", "Template");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> BlockUsers(List<string> userIds)
    {
        if(userIds == null || !userIds.Any())
        {
            TempData["AlertMessage"] = "No users selected.";
            TempData["AlertType"] = "danger";
            return RedirectToAction(nameof(Index));
        }
 
        var usersToBlock = await _userManager.Users
            .Where(user => userIds.Contains(user.Email))
            .ToListAsync();
        var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

        foreach (var user in usersToBlock)
        {
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            user.IsBlocked = true;
            user.LockoutEnabled = true;
            await _userManager.UpdateAsync(user);
            if (user.Email.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase))
            {
                await _signInManager.SignOutAsync();
            }
        }

        TempData["AlertMessage"] = $"{usersToBlock.Count} user(s) blocked successfully.";
        TempData["AlertType"] = "success";
        return RedirectToAction("AllTemplate", "Template");
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UnBlockUsers(List<string> userIds)
    {
        if (userIds == null || !userIds.Any())
        {
            TempData["AlertMessage"] = "No users selected.";
            TempData["AlertType"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        var usersToUnBlock = await _userManager.Users
            .Where(user => userIds.Contains(user.Email))
            .ToListAsync();

        foreach (var user in usersToUnBlock)
        {
            user.LockoutEnd = null;
            user.IsBlocked = false;
            user.LockoutEnabled = true;
            await _userManager.UpdateAsync(user);
        }
        TempData["AlertMessage"] = $"{usersToUnBlock.Count} user(s) unblocked successfully.";
        TempData["AlertType"] = "success";
        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeleteUsers(List<string> userIds)
    {
        if (userIds == null || !userIds.Any())
        {
            TempData["AlertMessage"] = "No users selected.";
            TempData["AlertType"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        var usersToDelete = await _userManager.Users
            .Where(user => userIds.Contains(user.Email))
            .ToListAsync();
        var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

        foreach (var user in usersToDelete)
        {
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {

                TempData["AlertMessage"] = "Failed to delete users";
                TempData["AlertType"] = "danger";
                return RedirectToAction(nameof(Index));
            }
            if (user.Email.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase))
            {
                await _signInManager.SignOutAsync();
            }
        }
        TempData["AlertMessage"] = "User(s) delted successfully.";
        TempData["AlertType"] = "success";
        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddToAdmins(List<string> userIds)
    {

        if (userIds == null || !userIds.Any())
        {
            TempData["AlertMessage"] = "No users selected.";
            TempData["AlertType"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        var usersToAdmin = await _userManager.Users
            .Where(user => userIds.Contains(user.Email))
            .ToListAsync();
            
        foreach (var user in usersToAdmin)
        {
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }
        }

        TempData["AlertType"] = "success";
        TempData["AlertMessage"] = "Selected users have been made Admins.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveFromAdmins(List<string> userIds)
    {
        var currentUserEmail = User.Identity.Name;

        var usersToRemoveFromAdmins = await _userManager.Users
            .Where(user => userIds.Contains(user.Email))
            .ToListAsync();
        foreach (var user in usersToRemoveFromAdmins)
        {
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            }
        }

        TempData["WarningMessage"] = userIds.Contains(currentUserEmail)
            ? "You removed yourself from the Admin role. You may now lose access."
            : "Selected users removed from Admins.";

        return RedirectToAction("Index");
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
