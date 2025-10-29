using System.Data;
using System.Data.SqlClient;
using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;
using AdminDashboard.Infrastructure.Data;

namespace AdminDashboard.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public ProductRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        var products = new List<Product>();

        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = "SP_GetAllProducts";
        command.CommandType = CommandType.StoredProcedure;

        connection.Open();
        using var reader = await ((SqlCommand)command).ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            products.Add(MapFromReader(reader));
        }

        return products;
    }

    public async Task<Product?> GetByIdAsync(int productId)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = "SP_GetProductById";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@ProductId", productId));

        connection.Open();
        using var reader = await ((SqlCommand)command).ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }

        return null;
    }

    public async Task<int> CreateAsync(Product product)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = "SP_CreateProduct";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@ProductCode", product.ProductCode));
        command.Parameters.Add(new SqlParameter("@ProductName", product.ProductName));
        command.Parameters.Add(new SqlParameter("@Description", (object?)product.Description ?? DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Category", product.Category));
        command.Parameters.Add(new SqlParameter("@Price", product.Price));
        command.Parameters.Add(new SqlParameter("@Stock", product.Stock));
        command.Parameters.Add(new SqlParameter("@IsActive", product.IsActive));
        command.Parameters.Add(new SqlParameter("@CreatedBy", product.CreatedBy));

        connection.Open();
        var result = await ((SqlCommand)command).ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<int> UpdateAsync(Product product)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = "SP_UpdateProduct";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@ProductId", product.ProductId));
        command.Parameters.Add(new SqlParameter("@ProductCode", product.ProductCode));
        command.Parameters.Add(new SqlParameter("@ProductName", product.ProductName));
        command.Parameters.Add(new SqlParameter("@Description", (object?)product.Description ?? DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Category", product.Category));
        command.Parameters.Add(new SqlParameter("@Price", product.Price));
        command.Parameters.Add(new SqlParameter("@Stock", product.Stock));
        command.Parameters.Add(new SqlParameter("@IsActive", product.IsActive));
        command.Parameters.Add(new SqlParameter("@ModifiedBy", product.ModifiedBy));

        connection.Open();
        var result = await ((SqlCommand)command).ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<int> DeleteAsync(int productId)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = "SP_DeleteProduct";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@ProductId", productId));

        connection.Open();
        var result = await ((SqlCommand)command).ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<IEnumerable<Product>> SearchAsync(string? searchTerm, string? category)
    {
        var products = new List<Product>();

        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = "SP_SearchProducts";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@SearchTerm", (object?)searchTerm ?? DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Category", (object?)category ?? DBNull.Value));

        connection.Open();
        using var reader = await ((SqlCommand)command).ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            products.Add(MapFromReader(reader));
        }

        return products;
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
