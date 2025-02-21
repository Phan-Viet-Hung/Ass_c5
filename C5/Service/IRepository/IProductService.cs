using C5.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace C5.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProducts();
        Task<Product> GetProductById(string id);
        Task<bool> CreateProduct(Product product, IFormFile imageFile);
        Task<bool> UpdateProduct(string id, Product updatedProduct, IFormFile imageFile);
        Task<bool> ToggleProductStatus(string id);
    }
}
