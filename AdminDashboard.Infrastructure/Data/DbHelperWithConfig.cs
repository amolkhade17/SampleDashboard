using System.Data;
using System.Data.SqlClient;
using AdminDashboard.Infrastructure.Data.Models;
using Microsoft.Extensions.Configuration;

namespace AdminDashboard.Infrastructure.Data;

/// <summary>
/// Enhanced DbHelper that works with JSON-based stored procedure configuration
/// </summary>
public class DbHelperWithConfig
{
    private readonly string _connectionString;
    private readonly IStoredProcedureConfigService _configService;

    public DbHelperWithConfig(IConfiguration configuration, IStoredProcedureConfigService configService)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(_connectionString));
        _configService = configService;
    }

    /// <summary>
    /// Execute stored procedure by entity and operation name
    /// Parameters are automatically configured from JSON
    /// </summary>
    public async Task<T?> ExecuteSingleAsync<T>(
        string entity,
        string operation,
        Func<SqlDataReader, T> mapper,
        Dictionary<string, object?>? parameterValues = null) where T : class
    {
        var config = _configService.GetProcedureConfig(entity, operation);
        if (config == null)
        {
            throw new InvalidOperationException($"Stored procedure not found: {entity}.{operation}");
        }

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(config.ProcedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        // Add parameters from configuration
        AddParametersFromConfig(command, config.Parameters, parameterValues);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return mapper(reader);
        }

        return null;
    }

    /// <summary>
    /// Execute stored procedure and return list
    /// </summary>
    public async Task<List<T>> ExecuteListAsync<T>(
        string entity,
        string operation,
        Func<SqlDataReader, T> mapper,
        Dictionary<string, object?>? parameterValues = null) where T : class
    {
        var results = new List<T>();
        var config = _configService.GetProcedureConfig(entity, operation);
        if (config == null)
        {
            throw new InvalidOperationException($"Stored procedure not found: {entity}.{operation}");
        }

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(config.ProcedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        AddParametersFromConfig(command, config.Parameters, parameterValues);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(mapper(reader));
        }

        return results;
    }

    /// <summary>
    /// Execute non-query stored procedure (INSERT/UPDATE/DELETE)
    /// </summary>
    public async Task<int> ExecuteNonQueryAsync(
        string entity,
        string operation,
        Dictionary<string, object?>? parameterValues = null)
    {
        var config = _configService.GetProcedureConfig(entity, operation);
        if (config == null)
        {
            throw new InvalidOperationException($"Stored procedure not found: {entity}.{operation}");
        }

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(config.ProcedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        AddParametersFromConfig(command, config.Parameters, parameterValues);

        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Execute stored procedure with output parameter
    /// </summary>
    public async Task<TOutput> ExecuteWithOutputAsync<TOutput>(
        string entity,
        string operation,
        Dictionary<string, object?>? parameterValues = null)
    {
        var config = _configService.GetProcedureConfig(entity, operation);
        if (config == null)
        {
            throw new InvalidOperationException($"Stored procedure not found: {entity}.{operation}");
        }

        if (string.IsNullOrEmpty(config.OutputParameter))
        {
            throw new InvalidOperationException($"No output parameter defined for {entity}.{operation}");
        }

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(config.ProcedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        AddParametersFromConfig(command, config.Parameters, parameterValues);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();

        // Get output parameter value
        var outputParam = command.Parameters[config.OutputParameter];
        return (TOutput)outputParam.Value;
    }

    /// <summary>
    /// Execute scalar query
    /// </summary>
    public async Task<T?> ExecuteScalarAsync<T>(
        string entity,
        string operation,
        Dictionary<string, object?>? parameterValues = null)
    {
        var config = _configService.GetProcedureConfig(entity, operation);
        if (config == null)
        {
            throw new InvalidOperationException($"Stored procedure not found: {entity}.{operation}");
        }

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(config.ProcedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        AddParametersFromConfig(command, config.Parameters, parameterValues);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        if (result == null || result == DBNull.Value)
        {
            return default;
        }

        return (T)result;
    }

    /// <summary>
    /// Execute dynamic query (for reports)
    /// </summary>
    public async Task<IEnumerable<dynamic>> ExecuteDynamicAsync(
        string entity,
        string operation,
        Dictionary<string, object?>? parameterValues = null)
    {
        var results = new List<dynamic>();
        var config = _configService.GetProcedureConfig(entity, operation);
        if (config == null)
        {
            throw new InvalidOperationException($"Stored procedure not found: {entity}.{operation}");
        }

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(config.ProcedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        AddParametersFromConfig(command, config.Parameters, parameterValues);

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
    /// Add parameters to command based on configuration
    /// </summary>
    private void AddParametersFromConfig(
        SqlCommand command,
        List<ParameterConfig> paramConfigs,
        Dictionary<string, object?>? parameterValues)
    {
        foreach (var paramConfig in paramConfigs)
        {
            var sqlType = _configService.GetSqlDbType(paramConfig.Type);
            var parameter = new SqlParameter(paramConfig.Name, sqlType);

            // Set size if specified
            if (paramConfig.Size.HasValue && paramConfig.Size.Value > 0)
            {
                parameter.Size = paramConfig.Size.Value;
            }
            else if (paramConfig.Size.HasValue && paramConfig.Size.Value == -1)
            {
                // -1 means MAX
                parameter.Size = -1;
            }

            // Set direction
            if (paramConfig.Direction.Equals("Output", StringComparison.OrdinalIgnoreCase))
            {
                parameter.Direction = ParameterDirection.Output;
            }
            else if (paramConfig.Direction.Equals("InputOutput", StringComparison.OrdinalIgnoreCase))
            {
                parameter.Direction = ParameterDirection.InputOutput;
            }
            else
            {
                parameter.Direction = ParameterDirection.Input;

                // Set value for input parameters
                if (parameterValues != null && parameterValues.TryGetValue(paramConfig.Name, out var value))
                {
                    parameter.Value = value ?? DBNull.Value;
                }
                else if (parameter.Direction == ParameterDirection.Input)
                {
                    // If no value provided for input parameter, set to DBNull
                    parameter.Value = DBNull.Value;
                }
            }

            command.Parameters.Add(parameter);
        }
    }

    /// <summary>
    /// Get procedure information for debugging
    /// </summary>
    public StoredProcedureConfig? GetProcedureInfo(string entity, string operation)
    {
        return _configService.GetProcedureConfig(entity, operation);
    }
}
