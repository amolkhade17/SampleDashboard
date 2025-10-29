using System.Data;
using System.Data.SqlClient;
using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;
using AdminDashboard.Infrastructure.Data;

namespace AdminDashboard.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DbHelperWithConfig _dbHelper;

    public ProductRepository(DbHelperWithConfig dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _dbHelper.ExecuteListAsync<Product>(
            entity: "Product",
            operation: "GetAll",
            mapper: MapFromReader
        );
    }

    public async Task<Product?> GetByIdAsync(int productId)
    {
        return await _dbHelper.ExecuteSingleAsync<Product>(
            entity: "Product",
            operation: "GetById",
            mapper: MapFromReader,
            parameterValues: new Dictionary<string, object?>
            {
                { "@ProductId", productId }
            }
        );
    }

    public async Task<int> CreateAsync(Product product)
    {
        return await _dbHelper.ExecuteWithOutputAsync<int>(
            entity: "Product",
            operation: "Create",
            parameterValues: new Dictionary<string, object?>
            {
                { "@ProductCode", product.ProductCode },
                { "@ProductName", product.ProductName },
                { "@Description", product.Description },
                { "@Category", product.Category },
                { "@Price", product.Price },
                { "@Stock", product.Stock },
                { "@IsActive", product.IsActive },
                { "@CreatedBy", product.CreatedBy }
            }
        );
    }

    public async Task<int> UpdateAsync(Product product)
    {
        return await _dbHelper.ExecuteNonQueryAsync(
            entity: "Product",
            operation: "Update",
            parameterValues: new Dictionary<string, object?>
            {
                { "@ProductId", product.ProductId },
                { "@ProductCode", product.ProductCode },
                { "@ProductName", product.ProductName },
                { "@Description", product.Description },
                { "@Category", product.Category },
                { "@Price", product.Price },
                { "@Stock", product.Stock },
                { "@IsActive", product.IsActive },
                { "@ModifiedBy", product.ModifiedBy }
            }
        );
    }

    public async Task<int> DeleteAsync(int productId)
    {
        return await _dbHelper.ExecuteNonQueryAsync(
            entity: "Product",
            operation: "Delete",
            parameterValues: new Dictionary<string, object?>
            {
                { "@ProductId", productId }
            }
        );
    }

    public async Task<IEnumerable<Product>> SearchAsync(string? searchTerm, string? category)
    {
        // Use GetByCategory if only category is provided, otherwise GetAll and filter in memory
        if (!string.IsNullOrEmpty(category) && string.IsNullOrEmpty(searchTerm))
        {
            return await _dbHelper.ExecuteListAsync<Product>(
                entity: "Product",
                operation: "GetByCategory",
                mapper: MapFromReader,
                parameterValues: new Dictionary<string, object?>
                {
                    { "@Category", category }
                }
            );
        }

        // For search term or combined search, get all and filter
        var allProducts = await GetAllAsync();
        
        if (!string.IsNullOrEmpty(searchTerm))
        {
            allProducts = allProducts.Where(p => 
                (p.ProductName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.ProductCode?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        
        if (!string.IsNullOrEmpty(category))
        {
            allProducts = allProducts.Where(p => 
                p.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        return allProducts;
    }

    private static Product MapFromReader(IDataReader reader)
    {
        return new Product
        {
            ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
            ProductCode = reader.GetString(reader.GetOrdinal("ProductCode")),
            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            Category = reader.GetString(reader.GetOrdinal("Category")),
            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
            Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedBy = reader.GetString(reader.GetOrdinal("CreatedBy")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
            ModifiedBy = reader.IsDBNull(reader.GetOrdinal("ModifiedBy")) ? null : reader.GetString(reader.GetOrdinal("ModifiedBy")),
            ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ModifiedDate"))
        };
    }
}
