using C5.Data;
using C5.Models;
using C5.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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

        // Hiển thị trang đăng ký
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý đăng ký tài khoản
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                TempData["Error"] = "Email đã được sử dụng. Vui lòng chọn email khác.";
                return View(model);
            }

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
                    UserId = user.Id // CartId = UserId
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đăng ký thành công! Hãy đăng nhập.";
                return RedirectToAction("Login");
            }

            foreach (var item in result.Errors)
            {
                ModelState.AddModelError("", item.Description);
            }

            return View(model);
        }

        // Hiển thị trang đăng nhập
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Xử lý đăng nhập
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }

            var appUser = await _userManager.FindByEmailAsync(login.Email);
            if (appUser == null)
            {
                TempData["Error"] = "Email không tồn tại.";
                return RedirectToAction("Login");
            }

            await _signInManager.SignOutAsync(); // Đảm bảo không có phiên đăng nhập cũ

            var result = await _signInManager.PasswordSignInAsync(appUser, login.Password, false, false);

            if (result.Succeeded)
            {
                TempData["Success"] = "Đăng nhập thành công!";
                return RedirectToAction("Index", "Home");
            }
            else if (result.IsLockedOut)
            {
                TempData["Error"] = "Tài khoản đã bị khóa. Vui lòng thử lại sau.";
            }
            else if (result.IsNotAllowed)
            {
                TempData["Error"] = "Bạn không có quyền đăng nhập.";
            }
            else
            {
                TempData["Error"] = "Mật khẩu không đúng.";
            }

            return RedirectToAction("Login");
        }

        // Đăng xuất tài khoản
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // Hiển thị thông tin tài khoản
        public async Task<IActionResult> DetailsUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để xem thông tin tài khoản.";
                return RedirectToAction("Login");
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

        // Hiển thị trang chỉnh sửa tài khoản
        [HttpGet]
        public async Task<IActionResult> EditUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return RedirectToAction("Login");
            }

            var model = new EditUserViewModel
            {
                FullName = user.FullName,
                DateOfBirth = user.DateOfBirth,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };

            return View(model);
        }


        // Xử lý cập nhật thông tin tài khoản
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Error"] = "Bạn cần đăng nhập để chỉnh sửa thông tin.";
                return RedirectToAction("Login");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return RedirectToAction("Login");
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
