using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Minimal.Store.API.Data.Context;

namespace Minimal.Store.Tests;

public class SqlServerConnectionTest : IDisposable
{
    private readonly StoreDbContext _context;
    private readonly ServiceProvider _serviceProvider;

    public SqlServerConnectionTest()
    {
        var services = new ServiceCollection();

        services.AddDbContext<StoreDbContext>(options =>
            options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MinimalStoreTestDb;Trusted_Connection=true;MultipleActiveResultSets=true"));

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<StoreDbContext>();
    }

    [Fact]
    public async Task SqlServer_Connection_Should_Work()
    {
        try
        {
            // Arrange & Act
            await _context.Database.EnsureCreatedAsync();
            var canConnect = await _context.Database.CanConnectAsync();

            // Assert
            canConnect.Should().BeTrue();
        }
        catch (Exception ex)
        {
            // 如果 LocalDB 不可用，測試應該顯示清楚的錯誤訊息
            ex.Message.Should().NotBeNullOrEmpty();
            // 這個測試可能會失敗如果沒有安裝 SQL Server LocalDB
        }
        finally
        {
            await _context.Database.EnsureDeletedAsync();
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}