using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AdminDashboard.Infrastructure.Data;

/// <summary>
/// Generic database helper for executing stored procedures with ADO.NET
/// Eliminates code duplication across repositories
/// </summary>
public class DbHelper
{
    private readonly string _connectionString;

    public DbHelper(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(_connectionString));
    }

    /// <summary>
    /// Execute a stored procedure and return a single entity
    /// </summary>
    public async Task<T?> ExecuteStoredProcedureSingleAsync<T>(
        string procedureName,
        Func<SqlDataReader, T> mapper,
        params SqlParameter[] parameters) where T : class
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (parameters != null && parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return mapper(reader);
        }

        return null;
    }

    /// <summary>
    /// Execute a stored procedure and return a list of entities
    /// </summary>
    public async Task<List<T>> ExecuteStoredProcedureListAsync<T>(
        string procedureName,
        Func<SqlDataReader, T> mapper,
        params SqlParameter[] parameters) where T : class
    {
        var results = new List<T>();

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (parameters != null && parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(mapper(reader));
        }

        return results;
    }

    /// <summary>
    /// Execute a stored procedure and return the number of affected rows
    /// </summary>
    public async Task<int> ExecuteStoredProcedureNonQueryAsync(
        string procedureName,
        params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (parameters != null && parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Execute a stored procedure with an output parameter
    /// </summary>
    public async Task<TOutput> ExecuteStoredProcedureWithOutputAsync<TOutput>(
        string procedureName,
        string outputParameterName,
        SqlDbType outputParameterType,
        params SqlParameter[] inputParameters)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (inputParameters != null && inputParameters.Length > 0)
        {
            command.Parameters.AddRange(inputParameters);
        }

        var outputParam = new SqlParameter(outputParameterName, outputParameterType)
        {
            Direction = ParameterDirection.Output
        };
        command.Parameters.Add(outputParam);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();

        return (TOutput)outputParam.Value;
    }

    /// <summary>
    /// Execute a stored procedure and return a scalar value
    /// </summary>
    public async Task<T?> ExecuteStoredProcedureScalarAsync<T>(
        string procedureName,
        params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (parameters != null && parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        if (result == null || result == DBNull.Value)
        {
            return default;
        }

        return (T)result;
    }

    /// <summary>
    /// Execute a stored procedure that returns dynamic results (for reports)
    /// </summary>
    public async Task<IEnumerable<dynamic>> ExecuteStoredProcedureDynamicAsync(
        string procedureName,
        params SqlParameter[] parameters)
    {
        var results = new List<dynamic>();

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (parameters != null && parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var expandoObject = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;

            for (int i = 0; i < reader.FieldCount; i++)
            {
                expandoObject.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i));
            }

            results.Add(expandoObject);
        }

        return results;
    }

    /// <summary>
    /// Execute stored procedure within a transaction
    /// </summary>
    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<SqlConnection, SqlTransaction, Task<T>> operation)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            var result = await operation(connection, transaction);
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Create a SqlParameter helper
    /// </summary>
    public static SqlParameter CreateParameter(string name, object? value, SqlDbType? type = null)
    {
        var parameter = new SqlParameter(name, value ?? DBNull.Value);
        if (type.HasValue)
        {
            parameter.SqlDbType = type.Value;
        }
        return parameter;
    }

    /// <summary>
    /// Create an output parameter
    /// </summary>
    public static SqlParameter CreateOutputParameter(string name, SqlDbType type, int? size = null)
    {
        var parameter = new SqlParameter(name, type)
        {
            Direction = ParameterDirection.Output
        };

        if (size.HasValue)
        {
            parameter.Size = size.Value;
        }

        return parameter;
    }

    /// <summary>
    /// Bulk insert helper (for future use)
    /// </summary>
    public async Task BulkInsertAsync<T>(string tableName, IEnumerable<T> data)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var bulkCopy = new SqlBulkCopy(connection);
        bulkCopy.DestinationTableName = tableName;

        // Note: Requires DataTable conversion - implement based on your needs
        throw new NotImplementedException("Bulk insert requires DataTable conversion");
    }
}
