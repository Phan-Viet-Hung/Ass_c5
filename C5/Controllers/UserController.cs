using C5.Data;
using C5.Models;
using C5.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace C5.Controllers
{
    [Authorize(Roles ="Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<FastFoodUser> _userManager;
        public UserController(UserManager<FastFoodUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> ListUser()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.Users
                .Include(u => u.Orders)
                .Include(u => u.Reviews)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userDetails = new
            {
                user.FullName,
                user.Email,
                user.DateOfBirth,
                user.CreatedAt,
                user.Address,
                TotalOrders = user.Orders.Count(),
                Roles = roles.ToList(), // Danh sách quyền
                Reviews = user.Reviews.Select(r => new
                {
                    r.Id,
                    r.Comment,
                    r.Rating
                }).ToList()
            };

            return View(userDetails);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var userId = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (userId == null)
            {
                TempData["NotFound"] = "Không tìm thấy Id này";
                return View("Index");
            }
            else
            {
                return View(userId);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Edit(string id,EditUserViewModel model)
        {
            var userId = await _userManager.FindByIdAsync(id);
            if (userId == null)
            {
                TempData["NotFound"] = "Không tìm thấy Id này";
                return View("Index");
            }
            if (ModelState.IsValid)
            {
                userId.FullName = model.FullName;
                userId.CreatedAt = model.DateOfBirth;
                userId.Email = model.Email;
                userId.UserName = model.Email;
                userId.PhoneNumber = model.PhoneNumber;
                userId.Address = model.Address;

                var result = await _userManager.UpdateAsync(userId);
                if (result.Succeeded)
                {
                    TempData["Update"] = "Cập nhật user thành công";
                    return RedirectToAction("ListUser");
                }
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
            }
            return View(model);
        }
        public async Task<IActionResult> Delete(string id)
        {
            var userId = await _userManager.FindByIdAsync(id);
            if (userId == null)
            {
                TempData["NotFound"] = "Không tìm thấy Id này";
                return RedirectToAction("ListUser");
            }
            else
            {
                var result = await _userManager.DeleteAsync(userId);
                if (result.Succeeded)
                {
                    TempData["Delete"] = "Xóa user thành công";
                    return RedirectToAction("ListUser");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("", item.Description);
                    }
                    return RedirectToAction("ListUser");
                }
            }
        }
    }
}
