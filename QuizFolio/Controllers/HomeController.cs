using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizFolio.Migrations;
using QuizFolio.Models;
using QuizFolio.ViewModels;

namespace QuizFolio.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<Users> _userManager;
    private readonly SignInManager<Users> _signInManager;

    public HomeController(ILogger<HomeController> logger, UserManager<Users> userManager, SignInManager<Users> signInManager)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
    }
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();

        var model = users.Select(user => new DashboardViewModel
        {
            Name = user.FullName,
            Designation = user.Designation,
            Email = user.Email,
            LastLoginTIme = user.LoginTime,
            LockoutEnd = user.LockoutEnd,
            IsBlocked = user.IsBlocked
        })
            .OrderByDescending(vm => vm.LastLoginTIme)
            .ToList();
        return View(model);
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
        return RedirectToAction(nameof(Index));
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
    public IActionResult Formgenarator()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
