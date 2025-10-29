using System.Data.SqlClient;
using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;
using AdminDashboard.Infrastructure.Data;

namespace AdminDashboard.Infrastructure.Repositories;

/// <summary>
/// Refactored ProductRepository using generic DbHelper
/// </summary>
public class ProductRepositoryRefactored : IProductRepository
{
    private readonly DbHelper _dbHelper;

    public ProductRepositoryRefactored(DbHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<Product?> GetByIdAsync(int productId)
    {
        return await _dbHelper.ExecuteStoredProcedureSingleAsync(
            StoredProcedureNames.Product.GetById,
            MapProductFromReader,
            DbHelper.CreateParameter("@ProductId", productId)
        );
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _dbHelper.ExecuteStoredProcedureListAsync(
            StoredProcedureNames.Product.GetAll,
            MapProductFromReader
        );
    }

    public async Task<int> CreateAsync(Product product)
    {
        return await _dbHelper.ExecuteStoredProcedureWithOutputAsync<int>(
            StoredProcedureNames.Product.Create,
            "@ProductId",
            System.Data.SqlDbType.Int,
            DbHelper.CreateParameter("@ProductName", product.ProductName),
            DbHelper.CreateParameter("@Description", product.Description),
            DbHelper.CreateParameter("@Price", product.Price),
            DbHelper.CreateParameter("@Stock", product.Stock),
            DbHelper.CreateParameter("@CategoryId", product.CategoryId)
        );
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        var rowsAffected = await _dbHelper.ExecuteStoredProcedureNonQueryAsync(
            StoredProcedureNames.Product.Update,
            DbHelper.CreateParameter("@ProductId", product.ProductId),
            DbHelper.CreateParameter("@ProductName", product.ProductName),
            DbHelper.CreateParameter("@Description", product.Description),
            DbHelper.CreateParameter("@Price", product.Price),
            DbHelper.CreateParameter("@Stock", product.Stock),
            DbHelper.CreateParameter("@CategoryId", product.CategoryId)
        );

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int productId)
    {
        var rowsAffected = await _dbHelper.ExecuteStoredProcedureNonQueryAsync(
            StoredProcedureNames.Product.Delete,
            DbHelper.CreateParameter("@ProductId", productId)
        );

        return rowsAffected > 0;
    }

    public async Task<bool> UpdateStockAsync(int productId, int newStock)
    {
        var rowsAffected = await _dbHelper.ExecuteStoredProcedureNonQueryAsync(
            StoredProcedureNames.Product.UpdateStock,
            DbHelper.CreateParameter("@ProductId", productId),
            DbHelper.CreateParameter("@Stock", newStock)
        );

        return rowsAffected > 0;
    }

    private static Product MapProductFromReader(SqlDataReader reader)
    {
        return new Product
        {
            ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                ? null
                : reader.GetString(reader.GetOrdinal("Description")),
            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
            Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
            CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
            CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
            ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("ModifiedDate"))
        };
    }
}
