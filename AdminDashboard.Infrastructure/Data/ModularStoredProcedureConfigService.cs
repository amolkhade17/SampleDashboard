using System.Data;
using System.Text.Json;
using AdminDashboard.Infrastructure.Data.Models;
using Microsoft.Extensions.Configuration;

namespace AdminDashboard.Infrastructure.Data;

/// <summary>
/// Service to manage modular stored procedure configurations from multiple JSON files
/// Supports loading from directory (recommended for scalability) with fallback to single file
/// </summary>
public class ModularStoredProcedureConfigService : IStoredProcedureConfigService
{
    private readonly string _connectionString;
    private readonly string _configDirectory;
    private readonly string _fallbackFile;
    private Dictionary<string, Dictionary<string, StoredProcedureConfig>> _procedures;
    private readonly object _lockObject = new object();

    public ModularStoredProcedureConfigService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(_connectionString));
        
        // Set paths for configuration files
        _configDirectory = Path.Combine(AppContext.BaseDirectory, "Data", "StoredProcedures");
        _fallbackFile = Path.Combine(AppContext.BaseDirectory, "Data", "storedProcedures.json");
        
        _procedures = new Dictionary<string, Dictionary<string, StoredProcedureConfig>>(StringComparer.OrdinalIgnoreCase);
        
        LoadAllConfigurations();
    }

    /// <summary>
    /// Load all JSON configuration files from the directory
    /// Falls back to single file if directory doesn't exist
    /// </summary>
    private void LoadAllConfigurations()
    {
        lock (_lockObject)
        {
            _procedures.Clear();

            // Try loading from modular directory first
            if (Directory.Exists(_configDirectory))
            {
                var jsonFiles = Directory.GetFiles(_configDirectory, "*.json", SearchOption.TopDirectoryOnly);
                
                if (jsonFiles.Length > 0)
                {
                    Console.WriteLine($"[ModularStoredProcedureConfigService] Loading {jsonFiles.Length} configuration files from {_configDirectory}");
                    
                    foreach (var file in jsonFiles)
                    {
                        LoadConfigurationFile(file);
                    }
                    
                    Console.WriteLine($"[ModularStoredProcedureConfigService] Loaded {GetTotalProcedureCount()} procedures from {jsonFiles.Length} files");
                    return;
                }
            }

            // Fallback to single file
            if (File.Exists(_fallbackFile))
            {
                Console.WriteLine($"[ModularStoredProcedureConfigService] Directory not found, falling back to single file: {_fallbackFile}");
                LoadConfigurationFile(_fallbackFile);
                Console.WriteLine($"[ModularStoredProcedureConfigService] Loaded {GetTotalProcedureCount()} procedures from fallback file");
            }
            else
            {
                Console.WriteLine($"[ModularStoredProcedureConfigService] WARNING: No configuration files found!");
                Console.WriteLine($"  - Modular directory: {_configDirectory}");
                Console.WriteLine($"  - Fallback file: {_fallbackFile}");
            }
        }
    }

    /// <summary>
    /// Load a single JSON configuration file
    /// </summary>
    private void LoadConfigurationFile(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            var root = JsonSerializer.Deserialize<StoredProceduresConfiguration>(json, options);
            
            if (root?.StoredProcedures != null)
            {
                foreach (var entity in root.StoredProcedures)
                {
                    if (!_procedures.ContainsKey(entity.Key))
                    {
                        _procedures[entity.Key] = new Dictionary<string, StoredProcedureConfig>(StringComparer.OrdinalIgnoreCase);
                    }

                    foreach (var operation in entity.Value)
                    {
                        _procedures[entity.Key][operation.Key] = operation.Value;
                        Console.WriteLine($"  Loaded: {entity.Key}.{operation.Key} -> {operation.Value.ProcedureName}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ModularStoredProcedureConfigService] ERROR loading {filePath}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Get stored procedure configuration by entity and operation name
    /// </summary>
    public StoredProcedureConfig? GetProcedureConfig(string entity, string operation)
    {
        if (_procedures.TryGetValue(entity, out var operations))
        {
            if (operations.TryGetValue(operation, out var config))
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
    /// Get all procedures for a specific entity
    /// </summary>
    public Dictionary<string, StoredProcedureConfig>? GetEntityProcedures(string entity)
    {
        return _procedures.TryGetValue(entity, out var operations) ? operations : null;
    }

    /// <summary>
    /// Get all loaded entities
    /// </summary>
    public IEnumerable<string> GetAllEntities()
    {
        return _procedures.Keys;
    }

    /// <summary>
    /// Reload all configurations (useful for hot reload in development)
    /// </summary>
    public void ReloadConfigurations()
    {
        Console.WriteLine("[ModularStoredProcedureConfigService] Reloading all configurations...");
        LoadAllConfigurations();
    }

    /// <summary>
    /// Get total number of loaded procedures
    /// </summary>
    public int GetTotalProcedureCount()
    {
        return _procedures.Values.Sum(ops => ops.Count);
    }

    /// <summary>
    /// Get procedure count by entity
    /// </summary>
    public Dictionary<string, int> GetProcedureCountByEntity()
    {
        return _procedures.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Count
        );
    }

    /// <summary>
    /// Validate that all configured procedures exist in the database
    /// </summary>
    public async Task<Dictionary<string, bool>> ValidateAllProceduresAsync()
    {
        var results = new Dictionary<string, bool>();
        
        using var connection = new System.Data.SqlClient.SqlConnection(_connectionString);
        await connection.OpenAsync();

        foreach (var entity in _procedures)
        {
            foreach (var operation in entity.Value)
            {
                var procedureName = operation.Value.ProcedureName;
                var exists = await ProcedureExistsAsync(connection, procedureName);
                results[$"{entity.Key}.{operation.Key}"] = exists;
                
                if (!exists)
                {
                    Console.WriteLine($"[ModularStoredProcedureConfigService] WARNING: Procedure not found in database: {procedureName}");
                }
            }
        }

        return results;
    }

    private async Task<bool> ProcedureExistsAsync(System.Data.SqlClient.SqlConnection connection, string procedureName)
    {
        var query = "SELECT COUNT(*) FROM sys.objects WHERE type = 'P' AND name = @ProcedureName";
        using var command = new System.Data.SqlClient.SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ProcedureName", procedureName);
        
        var count = (int)(await command.ExecuteScalarAsync() ?? 0);
        return count > 0;
    }

    /// <summary>
    /// Convert string type name to SqlDbType
    /// </summary>
    public SqlDbType GetSqlDbType(string typeName)
    {
        return typeName.ToLower() switch
        {
            "int" => SqlDbType.Int,
            "bigint" => SqlDbType.BigInt,
            "smallint" => SqlDbType.SmallInt,
            "tinyint" => SqlDbType.TinyInt,
            "bit" => SqlDbType.Bit,
            "decimal" => SqlDbType.Decimal,
            "numeric" => SqlDbType.Decimal,
            "money" => SqlDbType.Money,
            "smallmoney" => SqlDbType.SmallMoney,
            "float" => SqlDbType.Float,
            "real" => SqlDbType.Real,
            "datetime" => SqlDbType.DateTime,
            "datetime2" => SqlDbType.DateTime2,
            "smalldatetime" => SqlDbType.SmallDateTime,
            "date" => SqlDbType.Date,
            "time" => SqlDbType.Time,
            "datetimeoffset" => SqlDbType.DateTimeOffset,
            "char" => SqlDbType.Char,
            "varchar" => SqlDbType.VarChar,
            "text" => SqlDbType.Text,
            "nchar" => SqlDbType.NChar,
            "nvarchar" => SqlDbType.NVarChar,
            "ntext" => SqlDbType.NText,
            "binary" => SqlDbType.Binary,
            "varbinary" => SqlDbType.VarBinary,
            "image" => SqlDbType.Image,
            "uniqueidentifier" => SqlDbType.UniqueIdentifier,
            "xml" => SqlDbType.Xml,
            _ => throw new ArgumentException($"Unsupported SQL type: {typeName}")
        };
    }
}
