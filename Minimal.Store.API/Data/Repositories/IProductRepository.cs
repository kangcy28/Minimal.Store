using Minimal.Store.API.Models.Entities;

namespace Minimal.Store.API.Data.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task<bool> HasProductsInCategoryAsync(int categoryId);
}