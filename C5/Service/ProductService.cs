using C5.Data;
using C5.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace C5.Services
{
    public class ProductService : IProductService
    {
        private readonly FastFoodDbContext _context;

        public ProductService(FastFoodDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            return await _context.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<Product> GetProductById(string id)
        {
            return await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> CreateProduct(Product product, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = UploadImage(imageFile);
                if (fileName == null) return false;
                product.Image = "/uploads/" + fileName;
            }

            product.CreatedAt = GetVietnamTime();
            _context.Products.Add(product);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateProduct(string id, Product updatedProduct, IFormFile imageFile)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            product.Description = updatedProduct.Description;
            product.CategoryId = updatedProduct.CategoryId;
            product.StockQuantity = updatedProduct.StockQuantity;

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = UploadImage(imageFile);
                if (fileName != null) product.Image = "/uploads/" + fileName;
            }

            _context.Products.Update(product);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleProductStatus(string id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.IsActive = !product.IsActive;
            return await _context.SaveChangesAsync() > 0;
        }

        private string UploadImage(IFormFile imageFile)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension)) return null;

            var fileName = Guid.NewGuid() + fileExtension;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(stream);
            }
            return fileName;
        }

        private static DateTime GetVietnamTime()
        {
            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
        }
    }
}
