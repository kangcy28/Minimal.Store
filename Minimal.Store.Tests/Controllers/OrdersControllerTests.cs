using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Minimal.Store.API.Controllers;
using Minimal.Store.API.Data.Repositories;
using Minimal.Store.API.Models.DTOs;
using Minimal.Store.API.Models.Entities;
using Minimal.Store.API.Services.Implementations;

namespace Minimal.Store.Tests.Controllers;

public class OrdersControllerTests : TestBase
{
    [Fact]
    public async Task CreateOrder_ShouldCalculateTotalAmount()
    {
        // Arrange
        var orderRepository = new OrderRepository(Context);
        var productRepository = new ProductRepository(Context);
        var orderService = new OrderService(orderRepository, productRepository);
        var controller = new OrdersController(orderService);

        // 先新增測試分類
        var category = new Category
        {
            Name = "Electronics",
            Description = "Electronic products",
            CreatedAt = DateTime.UtcNow
        };

        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        // 新增測試商品
        var product1 = new Product
        {
            Name = "iPhone 15",
            Description = "Latest iPhone model",
            Price = 999.99m,
            Stock = 100,
            CategoryId = category.Id,
            CreatedAt = DateTime.UtcNow
        };

        var product2 = new Product
        {
            Name = "iPad Pro",
            Description = "Professional tablet",
            Price = 799.99m,
            Stock = 50,
            CategoryId = category.Id,
            CreatedAt = DateTime.UtcNow
        };

        Context.Products.AddRange(product1, product2);
        await Context.SaveChangesAsync();

        var createOrderDto = new CreateOrderDto
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            OrderItems = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = product1.Id,
                    Quantity = 2
                },
                new CreateOrderItemDto
                {
                    ProductId = product2.Id,
                    Quantity = 1
                }
            }
        };

        // Act
        var result = await controller.Create(createOrderDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);

        var createdOrder = createdResult.Value.Should().BeOfType<OrderDto>().Subject;
        createdOrder.Id.Should().BeGreaterThan(0);
        createdOrder.CustomerName.Should().Be("John Doe");
        createdOrder.CustomerEmail.Should().Be("john@example.com");
        createdOrder.Status.Should().Be("Pending");

        // 驗證總金額計算正確：(999.99 * 2) + (799.99 * 1) = 2799.97
        createdOrder.TotalAmount.Should().Be(2799.97m);

        createdOrder.OrderItems.Should().HaveCount(2);

        var orderItem1 = createdOrder.OrderItems.First(oi => oi.ProductId == product1.Id);
        orderItem1.Quantity.Should().Be(2);
        orderItem1.UnitPrice.Should().Be(999.99m);
        orderItem1.TotalPrice.Should().Be(1999.98m);

        var orderItem2 = createdOrder.OrderItems.First(oi => oi.ProductId == product2.Id);
        orderItem2.Quantity.Should().Be(1);
        orderItem2.UnitPrice.Should().Be(799.99m);
        orderItem2.TotalPrice.Should().Be(799.99m);

        createdOrder.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateOrder_ShouldThrowException_WhenProductOutOfStock()
    {
        // Arrange
        var orderRepository = new OrderRepository(Context);
        var productRepository = new ProductRepository(Context);
        var orderService = new OrderService(orderRepository, productRepository);
        var controller = new OrdersController(orderService);

        // 先新增測試分類
        var category = new Category
        {
            Name = "Electronics",
            Description = "Electronic products",
            CreatedAt = DateTime.UtcNow
        };

        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        // 新增測試商品 - 庫存只有 1
        var product = new Product
        {
            Name = "iPhone 15",
            Description = "Latest iPhone model",
            Price = 999.99m,
            Stock = 1, // 只有 1 個庫存
            CategoryId = category.Id,
            CreatedAt = DateTime.UtcNow
        };

        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        var createOrderDto = new CreateOrderDto
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            OrderItems = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = product.Id,
                    Quantity = 2 // 要求 2 個，但庫存只有 1 個
                }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => controller.Create(createOrderDto));
        exception.Message.Should().Contain("Insufficient stock");
        exception.Message.Should().Contain("iPhone 15");
    }

    [Fact]
    public async Task GetAll_ShouldReturnOrderList()
    {
        // Arrange
        var orderRepository = new OrderRepository(Context);
        var productRepository = new ProductRepository(Context);
        var orderService = new OrderService(orderRepository, productRepository);
        var controller = new OrdersController(orderService);

        // 先新增測試分類
        var category = new Category
        {
            Name = "Electronics",
            Description = "Electronic products",
            CreatedAt = DateTime.UtcNow
        };

        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        // 新增測試商品
        var product = new Product
        {
            Name = "iPhone 15",
            Description = "Latest iPhone model",
            Price = 999.99m,
            Stock = 100,
            CategoryId = category.Id,
            CreatedAt = DateTime.UtcNow
        };

        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        // 建立測試訂單
        var order1 = new Order
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            TotalAmount = 999.99m,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = 1,
                    UnitPrice = 999.99m
                }
            }
        };

        var order2 = new Order
        {
            CustomerName = "Jane Smith",
            CustomerEmail = "jane@example.com",
            TotalAmount = 1999.98m,
            Status = "Completed",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = 2,
                    UnitPrice = 999.99m
                }
            }
        };

        Context.Orders.AddRange(order1, order2);
        await Context.SaveChangesAsync();

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var orders = okResult.Value.Should().BeAssignableTo<IEnumerable<OrderDto>>().Subject.ToList();
        orders.Should().HaveCount(2);

        var firstOrder = orders.First();
        firstOrder.CustomerName.Should().Be("John Doe");
        firstOrder.CustomerEmail.Should().Be("john@example.com");
        firstOrder.TotalAmount.Should().Be(999.99m);
        firstOrder.Status.Should().Be("Pending");

        var secondOrder = orders.Last();
        secondOrder.CustomerName.Should().Be("Jane Smith");
        secondOrder.CustomerEmail.Should().Be("jane@example.com");
        secondOrder.TotalAmount.Should().Be(1999.98m);
        secondOrder.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task GetById_ShouldReturnOrderWithProductDetails()
    {
        // Arrange
        var orderRepository = new OrderRepository(Context);
        var productRepository = new ProductRepository(Context);
        var orderService = new OrderService(orderRepository, productRepository);
        var controller = new OrdersController(orderService);

        // 先新增測試分類
        var category = new Category
        {
            Name = "Electronics",
            Description = "Electronic products",
            CreatedAt = DateTime.UtcNow
        };

        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        // 新增測試商品
        var product1 = new Product
        {
            Name = "iPhone 15",
            Description = "Latest iPhone model",
            Price = 999.99m,
            Stock = 100,
            CategoryId = category.Id,
            CreatedAt = DateTime.UtcNow
        };

        var product2 = new Product
        {
            Name = "iPad Pro",
            Description = "Professional tablet",
            Price = 799.99m,
            Stock = 50,
            CategoryId = category.Id,
            CreatedAt = DateTime.UtcNow
        };

        Context.Products.AddRange(product1, product2);
        await Context.SaveChangesAsync();

        // 建立測試訂單
        var order = new Order
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            TotalAmount = 2799.97m,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = product1.Id,
                    Quantity = 2,
                    UnitPrice = 999.99m
                },
                new OrderItem
                {
                    ProductId = product2.Id,
                    Quantity = 1,
                    UnitPrice = 799.99m
                }
            }
        };

        Context.Orders.Add(order);
        await Context.SaveChangesAsync();

        // Act
        var result = await controller.GetById(order.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var orderDto = okResult.Value.Should().BeOfType<OrderDto>().Subject;
        orderDto.Id.Should().Be(order.Id);
        orderDto.CustomerName.Should().Be("John Doe");
        orderDto.CustomerEmail.Should().Be("john@example.com");
        orderDto.TotalAmount.Should().Be(2799.97m);
        orderDto.Status.Should().Be("Pending");

        orderDto.OrderItems.Should().HaveCount(2);

        var orderItem1 = orderDto.OrderItems.First(oi => oi.ProductId == product1.Id);
        orderItem1.ProductName.Should().Be("iPhone 15");
        orderItem1.Quantity.Should().Be(2);
        orderItem1.UnitPrice.Should().Be(999.99m);

        var orderItem2 = orderDto.OrderItems.First(oi => oi.ProductId == product2.Id);
        orderItem2.ProductName.Should().Be("iPad Pro");
        orderItem2.Quantity.Should().Be(1);
        orderItem2.UnitPrice.Should().Be(799.99m);

        orderDto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderRepository = new OrderRepository(Context);
        var productRepository = new ProductRepository(Context);
        var orderService = new OrderService(orderRepository, productRepository);
        var controller = new OrdersController(orderService);

        var nonExistentOrderId = 999;

        // Act
        var result = await controller.GetById(nonExistentOrderId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}