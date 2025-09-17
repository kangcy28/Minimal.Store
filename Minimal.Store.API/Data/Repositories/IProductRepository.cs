using Minimal.Store.API.Models.Entities;

namespace Minimal.Store.API.Data.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<Product> CreateAsync(Product product);
}