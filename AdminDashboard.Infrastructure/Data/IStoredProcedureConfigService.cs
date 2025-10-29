using System.Data;
using AdminDashboard.Infrastructure.Data.Models;

namespace AdminDashboard.Infrastructure.Data;

/// <summary>
/// Interface for stored procedure configuration services
/// Supports both single-file and modular approaches
/// </summary>
public interface IStoredProcedureConfigService
{
    /// <summary>
    /// Get stored procedure configuration by entity and operation name
    /// </summary>
    StoredProcedureConfig? GetProcedureConfig(string entity, string operation);

    /// <summary>
    /// Try to get stored procedure configuration
    /// </summary>
    bool TryGetProcedureConfig(string entity, string operation, out StoredProcedureConfig? config);

    /// <summary>
    /// Get SqlDbType from string type name
    /// </summary>
    SqlDbType GetSqlDbType(string typeName);
}
