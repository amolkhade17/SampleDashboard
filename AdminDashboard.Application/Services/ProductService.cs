using AdminDashboard.Application.DTOs;
using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;

namespace AdminDashboard.Application.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int productId);
    Task<int> CreateProductAsync(CreateProductDto dto, string createdBy);
    Task<int> UpdateProductAsync(UpdateProductDto dto, string modifiedBy);
    Task<int> DeleteProductAsync(int productId);
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string? searchTerm, string? category);
}

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(MapToDto);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        return product == null ? null : MapToDto(product);
    }

    public async Task<int> CreateProductAsync(CreateProductDto dto, string createdBy)
    {
        var product = new Product
        {
            ProductCode = dto.ProductCode,
            ProductName = dto.ProductName,
            Description = dto.Description,
            Category = dto.Category,
            Price = dto.Price,
            Stock = dto.Stock,
            IsActive = dto.IsActive,
            CreatedBy = createdBy,
            CreatedDate = DateTime.Now
        };

        return await _productRepository.CreateAsync(product);
    }

    public async Task<int> UpdateProductAsync(UpdateProductDto dto, string modifiedBy)
    {
        var product = new Product
        {
            ProductId = dto.ProductId,
            ProductCode = dto.ProductCode,
            ProductName = dto.ProductName,
            Description = dto.Description,
            Category = dto.Category,
            Price = dto.Price,
            Stock = dto.Stock,
            IsActive = dto.IsActive,
            ModifiedBy = modifiedBy,
            ModifiedDate = DateTime.Now
        };

        return await _productRepository.UpdateAsync(product);
    }

    public async Task<int> DeleteProductAsync(int productId)
    {
        return await _productRepository.DeleteAsync(productId);
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string? searchTerm, string? category)
    {
        var products = await _productRepository.SearchAsync(searchTerm, category);
        return products.Select(MapToDto);
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            ProductId = product.ProductId,
            ProductCode = product.ProductCode,
            ProductName = product.ProductName,
            Description = product.Description,
            Category = product.Category,
            Price = product.Price,
            Stock = product.Stock,
            IsActive = product.IsActive,
            CreatedBy = product.CreatedBy,
            CreatedDate = product.CreatedDate,
            ModifiedBy = product.ModifiedBy,
            ModifiedDate = product.ModifiedDate
        };
    }
}
