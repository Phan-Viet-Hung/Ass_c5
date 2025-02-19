using C5.Data;
using C5.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace C5.Controllers
{
    public class NotificationController : Controller
    {
        private readonly FastFoodDbContext _context;
        private readonly UserManager<FastFoodUser> _userManager;
        public NotificationController(UserManager<FastFoodUser> userManager, FastFoodDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        [Authorize]
        public async Task<IActionResult> ListNotifications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var notifications = await _context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

    }
}
