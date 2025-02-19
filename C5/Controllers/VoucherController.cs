using C5.Data;
using C5.Models;
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
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateVoucher()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVoucher(Voucher model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Lỗi dữ liệu không hợp lệ";
                return View(model);
            }

            var existingVoucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.Code == model.Code);

            if (existingVoucher != null)
            {
                ModelState.AddModelError("Code", "Mã giảm giá đã tồn tại.");
                return View(model);
            }

            _context.Vouchers.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Tạo voucher thành công!";
            return RedirectToAction("ListVoucher");
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditVoucher(Guid id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }
            return View(voucher);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVoucher(Guid id, Voucher model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingVoucher = await _context.Vouchers.FindAsync(id);
            if (existingVoucher == null)
            {
                return NotFound();
            }

            existingVoucher.Code = model.Code;
            existingVoucher.DiscountPercent = model.DiscountPercent;
            existingVoucher.Quantity = model.Quantity;
            existingVoucher.StartDate = model.StartDate;
            existingVoucher.EndDate = model.EndDate;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật voucher thành công!";
            return RedirectToAction("ListVoucher");
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVoucher(Guid id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }

            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa voucher thành công!";
            return RedirectToAction("ListVoucher");
        }

    }
}
