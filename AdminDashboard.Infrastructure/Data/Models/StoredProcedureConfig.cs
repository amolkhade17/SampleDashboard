namespace AdminDashboard.Infrastructure.Data.Models;

/// <summary>
/// Represents stored procedure configuration from JSON
/// </summary>
public class StoredProcedureConfig
{
    public string ProcedureName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ParameterConfig> Parameters { get; set; } = new();
    public string ReturnType { get; set; } = "Single"; // Single, List, NonQuery, Scalar
    public string? OutputParameter { get; set; }
}

/// <summary>
/// Represents parameter configuration
/// </summary>
public class ParameterConfig
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Int, VarChar, NVarChar, Bit, Decimal, DateTime, etc.
    public int? Size { get; set; }
    public string Direction { get; set; } = "Input"; // Input, Output, InputOutput
}

/// <summary>
/// Root configuration for all stored procedures
/// </summary>
public class StoredProceduresConfiguration
{
    public Dictionary<string, Dictionary<string, StoredProcedureConfig>> StoredProcedures { get; set; } = new();
}
