using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Minimal.Store.API.Data.Context;

namespace Minimal.Store.Tests;

public abstract class TestBase : IDisposable
{
    protected readonly StoreDbContext Context;
    private readonly ServiceProvider _serviceProvider;

    protected TestBase()
    {
        var services = new ServiceCollection();

        services.AddDbContext<StoreDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        _serviceProvider = services.BuildServiceProvider();
        Context = _serviceProvider.GetRequiredService<StoreDbContext>();

        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}