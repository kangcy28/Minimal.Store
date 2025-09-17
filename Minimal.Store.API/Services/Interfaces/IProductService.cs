using Minimal.Store.API.Models.DTOs;

namespace Minimal.Store.API.Services.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync();
    Task<ProductDto> CreateAsync(CreateProductDto dto);
}