using Minimal.Store.API.Models.Entities;

namespace Minimal.Store.API.Data.Repositories;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order);
    Task<IEnumerable<Order>> GetAllAsync();
}