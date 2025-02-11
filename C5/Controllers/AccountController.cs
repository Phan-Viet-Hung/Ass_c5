﻿using C5.Data;
using C5.Models;
using C5.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace C5.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<FastFoodUser> _userManager;
        private readonly SignInManager<FastFoodUser> _signInManager;
        private readonly FastFoodDbContext _context;
        public AccountController(UserManager<FastFoodUser> userManager, SignInManager<FastFoodUser> signInManager, FastFoodDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
                var user = new FastFoodUser
                {
                    Id = Guid.NewGuid().ToString(), // Tạo Id trước để dùng cho cả User và Cart
                    FullName = model.FullName,
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");

                    // 🔹 Tạo giỏ hàng mới với cùng Id của User
                    var cart = new Cart
                    {
                        //Id = user.Id, // Giỏ hàng có cùng Id với User
                        UserId = user.Id
                    };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Login");
                }
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }

            // Tìm user theo email
            var appUser = await _userManager.FindByEmailAsync(login.Email);

            if (appUser == null)
            {
                TempData["Login"] = "Email không tồn tại";
                return RedirectToAction("Login");
            }

            // Kiểm tra nếu email chưa được xác nhận (nếu có yêu cầu xác nhận email)
            //if (!appUser.EmailConfirmed)
            //{
            //    TempData["Login"] = "Tài khoản chưa được xác nhận. Vui lòng kiểm tra email.";
            //    return RedirectToAction("Login");
            //}

            // Đăng xuất các phiên trước đó
            await _signInManager.SignOutAsync();

            // Kiểm tra đăng nhập
            var result = await _signInManager.PasswordSignInAsync(appUser, login.Password, false, false);

            if (result.Succeeded)
            {
                TempData["Login"] = "Chào mừng đã đến với Shop!";
                return RedirectToAction("Index", "Home");
            }
            else if (result.IsLockedOut)
            {
                TempData["Login"] = "Tài khoản của bạn đã bị khóa. Vui lòng thử lại sau.";
            }
            else if (result.IsNotAllowed)
            {
                TempData["Login"] = "Bạn không có quyền đăng nhập.";
            }
            else
            {
                TempData["Login"] = "Mật khẩu không đúng.";
            }

            return RedirectToAction("Login");
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home"); 
        }
        public async Task<IActionResult> DetailsUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new CreateUserViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> EditUser()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}

            // Kiểm tra người dùng có đang đăng nhập không
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Cập nhật thông tin người dùng
            user.FullName = model.FullName;
            user.DateOfBirth = model.DateOfBirth;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("DetailsUser");
            }

            // Nếu cập nhật thất bại, hiển thị lỗi
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }


    }
}
