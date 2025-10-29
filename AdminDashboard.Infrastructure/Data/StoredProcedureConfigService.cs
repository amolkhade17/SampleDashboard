using System.Data;
using AdminDashboard.Infrastructure.Data.Models;
using Microsoft.Extensions.Configuration;

namespace AdminDashboard.Infrastructure.Data;

/// <summary>
/// Service to load and manage stored procedure configurations from JSON
/// </summary>
public class StoredProcedureConfigService : IStoredProcedureConfigService
{
    private readonly StoredProceduresConfiguration _configuration;
    private readonly Dictionary<string, SqlDbType> _typeMapping;

    public StoredProcedureConfigService(IConfiguration configuration)
    {
        // Load configuration from JSON file
        _configuration = new StoredProceduresConfiguration();
        configuration.GetSection("StoredProcedures").Bind(_configuration.StoredProcedures);

        // Initialize SQL type mapping
        _typeMapping = new Dictionary<string, SqlDbType>(StringComparer.OrdinalIgnoreCase)
        {
            { "Int", SqlDbType.Int },
            { "BigInt", SqlDbType.BigInt },
            { "SmallInt", SqlDbType.SmallInt },
            { "TinyInt", SqlDbType.TinyInt },
            { "Bit", SqlDbType.Bit },
            { "Decimal", SqlDbType.Decimal },
            { "Money", SqlDbType.Money },
            { "Float", SqlDbType.Float },
            { "Real", SqlDbType.Real },
            { "DateTime", SqlDbType.DateTime },
            { "DateTime2", SqlDbType.DateTime2 },
            { "Date", SqlDbType.Date },
            { "Time", SqlDbType.Time },
            { "VarChar", SqlDbType.VarChar },
            { "NVarChar", SqlDbType.NVarChar },
            { "Char", SqlDbType.Char },
            { "NChar", SqlDbType.NChar },
            { "Text", SqlDbType.Text },
            { "NText", SqlDbType.NText },
            { "UniqueIdentifier", SqlDbType.UniqueIdentifier },
            { "Binary", SqlDbType.Binary },
            { "VarBinary", SqlDbType.VarBinary }
        };
    }

    /// <summary>
    /// Get stored procedure configuration by entity and operation
    /// </summary>
    public StoredProcedureConfig? GetProcedureConfig(string entity, string operation)
    {
        if (_configuration.StoredProcedures.TryGetValue(entity, out var entityProcedures))
        {
            if (entityProcedures.TryGetValue(operation, out var config))
            {
                return config;
            }
        }

        return null;
    }

    /// <summary>
    /// Try to get stored procedure configuration
    /// </summary>
    public bool TryGetProcedureConfig(string entity, string operation, out StoredProcedureConfig? config)
    {
        config = GetProcedureConfig(entity, operation);
        return config != null;
    }

    /// <summary>
    /// Get procedure name by entity and operation
    /// </summary>
    public string GetProcedureName(string entity, string operation)
    {
        var config = GetProcedureConfig(entity, operation);
        if (config == null)
        {
            throw new InvalidOperationException($"Stored procedure not found: {entity}.{operation}");
        }

        return config.ProcedureName;
    }

    /// <summary>
    /// Get all procedure configurations for an entity
    /// </summary>
    public Dictionary<string, StoredProcedureConfig>? GetEntityProcedures(string entity)
    {
        if (_configuration.StoredProcedures.TryGetValue(entity, out var procedures))
        {
            return procedures;
        }

        return null;
    }

    /// <summary>
    /// Convert string type to SqlDbType
    /// </summary>
    public SqlDbType GetSqlDbType(string typeName)
    {
        if (_typeMapping.TryGetValue(typeName, out var sqlType))
        {
            return sqlType;
        }

        throw new ArgumentException($"Unknown SQL type: {typeName}");
    }

    /// <summary>
    /// Get all available entities
    /// </summary>
    public IEnumerable<string> GetAllEntities()
    {
        return _configuration.StoredProcedures.Keys;
    }

    /// <summary>
    /// Get all operations for an entity
    /// </summary>
    public IEnumerable<string> GetEntityOperations(string entity)
    {
        var procedures = GetEntityProcedures(entity);
        return procedures?.Keys ?? Enumerable.Empty<string>();
    }

    /// <summary>
    /// Validate if a procedure exists
    /// </summary>
    public bool ProcedureExists(string entity, string operation)
    {
        return GetProcedureConfig(entity, operation) != null;
    }

    /// <summary>
    /// Get procedure description
    /// </summary>
    public string GetProcedureDescription(string entity, string operation)
    {
        var config = GetProcedureConfig(entity, operation);
        return config?.Description ?? string.Empty;
    }

    /// <summary>
    /// Get all stored procedures (for debugging/documentation)
    /// </summary>
    public Dictionary<string, Dictionary<string, StoredProcedureConfig>> GetAllProcedures()
    {
        return _configuration.StoredProcedures;
    }
}
