using Minimal.Store.API.Models.DTOs;

namespace Minimal.Store.API.Services.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
}