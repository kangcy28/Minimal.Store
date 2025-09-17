using Minimal.Store.API.Models.DTOs;

namespace Minimal.Store.API.Services.Interfaces;

public interface IProductService
{
    Task<ProductDto> CreateAsync(CreateProductDto dto);
}