using Microsoft.EntityFrameworkCore;
using Minimal.Store.API.Data.Context;
using Minimal.Store.API.Models.Entities;

namespace Minimal.Store.API.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly StoreDbContext _context;

    public OrderRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // 載入關聯的資料
        await _context.Entry(order)
            .Collection(o => o.OrderItems)
            .Query()
            .Include(oi => oi.Product)
            .LoadAsync();

        return order;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order?> UpdateStatusAsync(int id, string status)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return null;

        order.Status = status;
        await _context.SaveChangesAsync();

        return order;
    }
}