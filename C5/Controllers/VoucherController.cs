using C5.Data;
using C5.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace C5.Controllers
{
    public class VoucherController : Controller
    {
        private readonly FastFoodDbContext _context;
        public VoucherController(FastFoodDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin")] // Chỉ Admin mới có quyền truy cập
        public async Task<IActionResult> ListVoucher(int page = 1, int pageSize = 10)
        {
            var vouchers = _context.Vouchers
                .OrderByDescending(v => v.EndDate); // Sắp xếp theo ngày hết hạn giảm dần

            var pagedVouchers = await vouchers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(); // Phân trang

            int totalVouchers = await vouchers.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalVouchers / pageSize);

            var viewModel = new VoucherListViewModel
            {
                Vouchers = pagedVouchers,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

    }
}
