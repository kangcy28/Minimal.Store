using Microsoft.EntityFrameworkCore;
using Minimal.Store.API.Data.Context;
using Minimal.Store.API.Models.Entities;

namespace Minimal.Store.API.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly StoreDbContext _context;

    public ProductRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .ToListAsync();
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // 載入關聯的 Category 資料
        await _context.Entry(product)
            .Reference(p => p.Category)
            .LoadAsync();

        return product;
    }
}