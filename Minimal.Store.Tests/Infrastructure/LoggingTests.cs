using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using Minimal.Store.API.Controllers;
using Minimal.Store.API.Models.DTOs;

namespace Minimal.Store.Tests.Infrastructure;

public class LoggingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly TestLoggerProvider _loggerProvider;

    public LoggingTests(WebApplicationFactory<Program> factory)
    {
        _loggerProvider = new TestLoggerProvider();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ILoggerProvider>(_loggerProvider);
            });
        });
    }

    [Fact]
    public async Task CategoryController_CreateCategory_Should_Log_Operation()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createCategoryDto = new CreateCategoryDto
        {
            Name = "Test Category",
            Description = "Test Description"
        };
        var jsonContent = JsonSerializer.Serialize(createCategoryDto);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/categories", content);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        _loggerProvider.LogEntries.Should().Contain(log =>
            log.LogLevel == LogLevel.Information &&
            log.Message.Contains("Creating category") &&
            log.Message.Contains("Test Category"));
    }

    [Fact]
    public async Task ProductController_GetProducts_Should_Log_Operation()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        _loggerProvider.LogEntries.Should().Contain(log =>
            log.LogLevel == LogLevel.Information &&
            log.Message.Contains("Retrieving products"));
    }

    [Fact]
    public async Task OrderController_CreateOrder_Should_Log_Business_Logic()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createOrderDto = new CreateOrderDto
        {
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            OrderItems = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = 1, Quantity = 2 }
            }
        };
        var jsonContent = JsonSerializer.Serialize(createOrderDto);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/orders", content);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        _loggerProvider.LogEntries.Should().Contain(log =>
            log.LogLevel == LogLevel.Information &&
            log.Message.Contains("Processing order creation"));
    }
}

public class TestLoggerProvider : ILoggerProvider
{
    public List<LogEntry> LogEntries { get; } = new();

    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger(this, categoryName);
    }

    public void Dispose() { }
}

public class TestLogger : ILogger
{
    private readonly TestLoggerProvider _provider;
    private readonly string _categoryName;

    public TestLogger(TestLoggerProvider provider, string categoryName)
    {
        _provider = provider;
        _categoryName = categoryName;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _provider.LogEntries.Add(new LogEntry
        {
            LogLevel = logLevel,
            EventId = eventId,
            Message = formatter(state, exception),
            Exception = exception,
            CategoryName = _categoryName
        });
    }
}

public class LogEntry
{
    public LogLevel LogLevel { get; set; }
    public EventId EventId { get; set; }
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}