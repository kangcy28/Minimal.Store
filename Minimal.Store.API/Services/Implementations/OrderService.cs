using Minimal.Store.API.Data.Repositories;
using Minimal.Store.API.Models.DTOs;
using Minimal.Store.API.Models.Entities;
using Minimal.Store.API.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Minimal.Store.API.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository orderRepository, IProductRepository productRepository, ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto)
    {
        _logger.LogInformation("Starting order creation process for customer: {CustomerName}", dto.CustomerName);

        var order = new Order
        {
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();

        _logger.LogInformation("Processing {ItemCount} order items", dto.OrderItems.Count);

        foreach (var itemDto in dto.OrderItems)
        {
            _logger.LogDebug("Validating product {ProductId} for quantity {Quantity}", itemDto.ProductId, itemDto.Quantity);
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId);

            if (product == null)
            {
                _logger.LogError("Product not found: {ProductId}", itemDto.ProductId);
                throw new ArgumentException($"Product with ID {itemDto.ProductId} not found");
            }

            // 檢查庫存是否足夠
            if (product.Stock < itemDto.Quantity)
            {
                _logger.LogWarning("Insufficient stock for product {ProductName}. Available: {AvailableStock}, Requested: {RequestedQuantity}", product.Name, product.Stock, itemDto.Quantity);
                throw new InvalidOperationException($"Insufficient stock for product '{product.Name}'. Available: {product.Stock}, Requested: {itemDto.Quantity}");
            }

            _logger.LogDebug("Stock validation passed for product {ProductName}", product.Name);

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

        _logger.LogInformation("Order validated successfully. Total amount: {TotalAmount}. Saving to database", totalAmount);
        var createdOrder = await _orderRepository.CreateAsync(order);
        _logger.LogInformation("Order created successfully with ID: {OrderId}", createdOrder.Id);

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

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);

        if (order == null)
            return null;

        return new OrderDto
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
        };
    }

    public async Task<OrderDto?> UpdateStatusAsync(int id, UpdateOrderStatusDto dto)
    {
        _logger.LogInformation("Updating order status for ID: {OrderId} to status: {NewStatus}", id, dto.Status);
        var order = await _orderRepository.UpdateStatusAsync(id, dto.Status);

        if (order == null)
        {
            _logger.LogWarning("Order not found for status update: {OrderId}", id);
            return null;
        }

        _logger.LogInformation("Order status updated successfully for ID: {OrderId}", id);

        return new OrderDto
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
        };
    }
}