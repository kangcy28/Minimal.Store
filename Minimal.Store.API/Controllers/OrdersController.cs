using Microsoft.AspNetCore.Mvc;
using Minimal.Store.API.Models.DTOs;
using Minimal.Store.API.Services.Interfaces;

namespace Minimal.Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Retrieving all orders");
        var orders = await _orderService.GetAllAsync();
        _logger.LogInformation("Retrieved {OrderCount} orders", orders.Count());
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("Retrieving order with ID: {OrderId}", id);
        var order = await _orderService.GetByIdAsync(id);

        if (order == null)
        {
            _logger.LogWarning("Order with ID: {OrderId} not found", id);
            return NotFound();
        }

        _logger.LogInformation("Successfully retrieved order for customer: {CustomerName}", order.CustomerName);
        return Ok(order);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusDto dto)
    {
        _logger.LogInformation("Updating order status for ID: {OrderId} to status: {NewStatus}", id, dto.Status);
        var order = await _orderService.UpdateStatusAsync(id, dto);

        if (order == null)
        {
            _logger.LogWarning("Order with ID: {OrderId} not found for status update", id);
            return NotFound();
        }

        _logger.LogInformation("Successfully updated order status for ID: {OrderId}", id);
        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        _logger.LogInformation("Processing order creation for customer: {CustomerName} with {ItemCount} items", dto.CustomerName, dto.OrderItems.Count);
        var orderDto = await _orderService.CreateAsync(dto);
        _logger.LogInformation("Successfully created order with ID: {OrderId} for total amount: {TotalAmount}", orderDto.Id, orderDto.TotalAmount);
        return CreatedAtAction("GetById", new { id = orderDto.Id }, orderDto);
    }
}