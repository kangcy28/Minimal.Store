using Microsoft.EntityFrameworkCore;
using Minimal.Store.API.Data.Context;
using Minimal.Store.API.Services.Interfaces;
using Minimal.Store.API.Services.Implementations;

namespace Minimal.Store.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<StoreDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICategoryService, CategoryService>();

        return services;
    }
}