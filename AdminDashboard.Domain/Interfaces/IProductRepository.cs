using AdminDashboard.Domain.Entities;

namespace AdminDashboard.Domain.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int productId);
    Task<int> CreateAsync(Product product);
    Task<int> UpdateAsync(Product product);
    Task<int> DeleteAsync(int productId);
    Task<IEnumerable<Product>> SearchAsync(string? searchTerm, string? category);
}
