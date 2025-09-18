using Minimal.Store.API.Data.Repositories;
using Minimal.Store.API.Models.DTOs;
using Minimal.Store.API.Models.Entities;
using Minimal.Store.API.Services.Interfaces;

namespace Minimal.Store.API.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto)
    {
        var order = new Order
        {
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();

        foreach (var itemDto in dto.OrderItems)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId);

            if (product == null)
                throw new ArgumentException($"Product with ID {itemDto.ProductId} not found");

            // 檢查庫存是否足夠
            if (product.Stock < itemDto.Quantity)
                throw new InvalidOperationException($"Insufficient stock for product '{product.Name}'. Available: {product.Stock}, Requested: {itemDto.Quantity}");

            var orderItem = new OrderItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price
            };

            totalAmount += orderItem.Quantity * orderItem.UnitPrice;
            orderItems.Add(orderItem);
        }

        order.TotalAmount = totalAmount;
        order.OrderItems = orderItems;

        var createdOrder = await _orderRepository.CreateAsync(order);

        return new OrderDto
        {
            Id = createdOrder.Id,
            CustomerName = createdOrder.CustomerName,
            CustomerEmail = createdOrder.CustomerEmail,
            TotalAmount = createdOrder.TotalAmount,
            Status = createdOrder.Status,
            CreatedAt = createdOrder.CreatedAt,
            OrderItems = createdOrder.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? string.Empty,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice
            }).ToList()
        };
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync()
    {
        var orders = await _orderRepository.GetAllAsync();

        return orders.Select(order => new OrderDto
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            OrderItems = order.OrderItems?.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? string.Empty,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice
            }).ToList() ?? new List<OrderItemDto>()
        });
    }
}