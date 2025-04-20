using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizFolio.Data;
using QuizFolio.Models;

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
        [Authorize]
        public async Task<IActionResult> ViewForm(int id)
        {
            var template = await _context.Templates
                            .Include(t => t.Creator)
                            .Include(t => t.Questions)
                            .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
                return NotFound();
            return View(template);
        }
    }
}
