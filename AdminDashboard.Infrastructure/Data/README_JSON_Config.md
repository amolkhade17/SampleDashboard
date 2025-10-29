# JSON-Based Stored Procedure Configuration - Complete Guide

## 🎯 Overview

This is the **ULTIMATE solution** for managing stored procedures in your application. Instead of hard-coding procedure names and parameters in C# code, everything is defined in a JSON configuration file.

## ✨ Key Benefits

### 1. **Zero Hard-Coding**
- ✅ No hard-coded procedure names
- ✅ No hard-coded parameter names
- ✅ No hard-coded parameter types
- ✅ No hard-coded parameter sizes

### 2. **Centralized Management**
- ✅ All procedures in one JSON file
- ✅ Easy to view all available procedures
- ✅ Easy to add new procedures
- ✅ Easy to modify existing procedures

### 3. **Self-Documenting**
- ✅ Description for each procedure
- ✅ Clear parameter definitions
- ✅ Type information visible
- ✅ No need to check database

### 4. **Environment-Specific**
- ✅ Different JSON files for Dev/QA/Prod
- ✅ Easy to switch between environments
- ✅ Version control friendly

### 5. **Validation & Safety**
- ✅ Compile-time validation through JSON schema
- ✅ Runtime validation of procedure existence
- ✅ Parameter type checking
- ✅ Clear error messages

## 📁 File Structure

```
AdminDashboard.Infrastructure/
├── Data/
│   ├── storedProcedures.json                   ⭐ JSON configuration
│   ├── Models/
│   │   └── StoredProcedureConfig.cs            ⭐ Configuration models
│   ├── StoredProcedureConfigService.cs         ⭐ Configuration loader
│   ├── DbHelperWithConfig.cs                   ⭐ Enhanced DbHelper
│   └── README_JSON_Config.md                   📖 This file
├── Repositories/
│   └── UserRepositoryJsonBased.cs              ⭐ Example implementation
```

## 🔧 Configuration Format

### JSON Structure
```json
{
  "StoredProcedures": {
    "EntityName": {
      "OperationName": {
        "ProcedureName": "SP_ActualDatabaseName",
        "Description": "What this procedure does",
        "Parameters": [
          {
            "Name": "@ParameterName",
            "Type": "SqlType",
            "Size": 100,
            "Direction": "Input|Output|InputOutput"
          }
        ],
        "ReturnType": "Single|List|NonQuery|Scalar",
        "OutputParameter": "@OutputParamName"
      }
    }
  }
}
```

### Supported SQL Types
- `Int`, `BigInt`, `SmallInt`, `TinyInt`
- `Bit`
- `Decimal`, `Money`, `Float`, `Real`
- `DateTime`, `DateTime2`, `Date`, `Time`
- `VarChar`, `NVarChar`, `Char`, `NChar`
- `Text`, `NText`
- `UniqueIdentifier`
- `Binary`, `VarBinary`

### Parameter Directions
- `Input` (default) - Input parameter
- `Output` - Output parameter
- `InputOutput` - Both input and output

### Return Types
- `Single` - Returns single entity (ExecuteSingleAsync)
- `List` - Returns list of entities (ExecuteListAsync)
- `NonQuery` - INSERT/UPDATE/DELETE (ExecuteNonQueryAsync)
- `Scalar` - Returns single value (ExecuteScalarAsync)

## 📝 Example Configurations

### Simple Select (No Parameters)
```json
"GetAll": {
  "ProcedureName": "SP_GetAllUsers",
  "Description": "Get all users",
  "Parameters": [],
  "ReturnType": "List"
}
```

### Select with Parameters
```json
"GetById": {
  "ProcedureName": "SP_GetUserById",
  "Description": "Get user by ID",
  "Parameters": [
    { "Name": "@UserId", "Type": "Int" }
  ],
  "ReturnType": "Single"
}
```

### Insert with Output Parameter
```json
"Create": {
  "ProcedureName": "SP_CreateUser",
  "Description": "Create new user",
  "Parameters": [
    { "Name": "@Username", "Type": "VarChar", "Size": 50 },
    { "Name": "@Email", "Type": "VarChar", "Size": 100 },
    { "Name": "@UserId", "Type": "Int", "Direction": "Output" }
  ],
  "ReturnType": "NonQuery",
  "OutputParameter": "@UserId"
}
```

### Update/Delete
```json
"Update": {
  "ProcedureName": "SP_UpdateUser",
  "Description": "Update existing user",
  "Parameters": [
    { "Name": "@UserId", "Type": "Int" },
    { "Name": "@Username", "Type": "VarChar", "Size": 50 },
    { "Name": "@Email", "Type": "VarChar", "Size": 100 }
  ],
  "ReturnType": "NonQuery"
}
```

### Large Text Fields (MAX)
```json
"CreateNote": {
  "ProcedureName": "SP_CreateNote",
  "Description": "Create note with large text",
  "Parameters": [
    { "Name": "@Content", "Type": "NVarChar", "Size": -1 }
  ],
  "ReturnType": "NonQuery"
}
```
*Note: Size = -1 means VARCHAR(MAX) or NVARCHAR(MAX)*

## 💻 Repository Implementation

### Step 1: Inject DbHelperWithConfig
```csharp
public class UserRepositoryJsonBased : IUserRepository
{
    private readonly DbHelperWithConfig _dbHelper;
    private const string Entity = "User";

    public UserRepositoryJsonBased(DbHelperWithConfig dbHelper)
    {
        _dbHelper = dbHelper;
    }
}
```

### Step 2: Use Entity and Operation Names
```csharp
// Get single entity
public async Task<User?> GetByIdAsync(int userId)
{
    return await _dbHelper.ExecuteSingleAsync(
        Entity,                    // "User"
        "GetById",                 // Operation name from JSON
        MapUserFromReader,         // Mapper function
        new Dictionary<string, object?>
        {
            { "@UserId", userId }  // Parameter values
        }
    );
}

// Get list
public async Task<IEnumerable<User>> GetAllAsync()
{
    return await _dbHelper.ExecuteListAsync(
        Entity,
        "GetAll",
        MapUserFromReader
        // No parameters needed
    );
}

// Create with output
public async Task<int> CreateAsync(User user)
{
    return await _dbHelper.ExecuteWithOutputAsync<int>(
        Entity,
        "Create",
        new Dictionary<string, object?>
        {
            { "@Username", user.Username },
            { "@Email", user.Email }
        }
    );
}

// Update/Delete
public async Task<bool> UpdateAsync(User user)
{
    var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(
        Entity,
        "Update",
        new Dictionary<string, object?>
        {
            { "@UserId", user.UserId },
            { "@Username", user.Username }
        }
    );
    
    return rowsAffected > 0;
}
```

## 🚀 Setup Instructions

### 1. Register Services in DI
```csharp
// In DependencyInjection.cs
services.AddSingleton<StoredProcedureConfigService>();
services.AddScoped<DbHelperWithConfig>();

// Use JSON-based repositories
services.AddScoped<IUserRepository, UserRepositoryJsonBased>();
```

### 2. Ensure JSON File is Copied
Already configured in `.csproj`:
```xml
<ItemGroup>
  <None Update="Data\storedProcedures.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

### 3. Build and Run
```bash
dotnet build
dotnet run
```

## 🔍 Debugging & Inspection

### Get Procedure Information
```csharp
var procInfo = _dbHelper.GetProcedureInfo("User", "GetById");
Console.WriteLine($"Procedure: {procInfo.ProcedureName}");
Console.WriteLine($"Description: {procInfo.Description}");
foreach (var param in procInfo.Parameters)
{
    Console.WriteLine($"  {param.Name} ({param.Type})");
}
```

### List All Available Procedures
```csharp
var configService = serviceProvider.GetService<StoredProcedureConfigService>();

foreach (var entity in configService.GetAllEntities())
{
    Console.WriteLine($"\nEntity: {entity}");
    foreach (var operation in configService.GetEntityOperations(entity))
    {
        var config = configService.GetProcedureConfig(entity, operation);
        Console.WriteLine($"  - {operation}: {config.Description}");
    }
}
```

## 📊 Code Comparison

### Traditional Approach (300 lines)
```csharp
public async Task<User?> GetByIdAsync(int userId)
{
    using var connection = new SqlConnection(_connectionString);
    using var command = new SqlCommand("SP_GetUserById", connection);  // Hard-coded
    command.CommandType = CommandType.StoredProcedure;
    
    command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) // Hard-coded
    {
        Value = userId
    });
    
    await connection.OpenAsync();
    using var reader = await command.ExecuteReaderAsync();
    
    if (await reader.ReadAsync())
    {
        return MapUserFromReader(reader);
    }
    
    return null;
}
```

### Constants Approach (150 lines)
```csharp
public async Task<User?> GetByIdAsync(int userId)
{
    return await _dbHelper.ExecuteStoredProcedureSingleAsync(
        StoredProcedureNames.User.GetById,  // Constant
        MapUserFromReader,
        DbHelper.CreateParameter("@UserId", userId)
    );
}
```

### JSON-Based Approach (140 lines + JSON config)
```csharp
public async Task<User?> GetByIdAsync(int userId)
{
    return await _dbHelper.ExecuteSingleAsync(
        "User",                     // Entity from JSON
        "GetById",                  // Operation from JSON
        MapUserFromReader,
        new Dictionary<string, object?> { { "@UserId", userId } }
    );
}
```

## ✅ Advantages Over Constants Approach

| Feature | Constants | JSON-Based |
|---------|-----------|------------|
| Procedure Names | C# constants | JSON config |
| Parameter Info | C# code | JSON config |
| Parameter Types | Manual | Automatic |
| Add New Procedure | Update C# + recompile | Edit JSON only |
| Environment-specific | Multiple constants files | Multiple JSON files |
| Documentation | Comments | Built-in descriptions |
| Runtime changes | Not possible | Possible (reload JSON) |
| Non-developers | Cannot edit | Can edit JSON |

## 🎓 Best Practices

### 1. Naming Conventions
```json
// Entity names: PascalCase
"User", "Product", "MakerChecker"

// Operation names: PascalCase
"GetById", "GetAll", "Create", "Update"

// Parameter names: Match database exactly
"@UserId", "@Username"
```

### 2. Organization
Group related procedures under same entity:
```json
{
  "StoredProcedures": {
    "User": {
      // All user-related procedures
    },
    "Product": {
      // All product-related procedures
    }
  }
}
```

### 3. Documentation
Always include descriptions:
```json
{
  "Description": "Get user by ID - includes role information"
}
```

### 4. Validation
Validate JSON structure on application startup:
```csharp
var configService = new StoredProcedureConfigService(configuration);
if (!configService.ProcedureExists("User", "GetById"))
{
    throw new Exception("Required procedure not configured");
}
```

## 🔄 Migration from Constants

### Step 1: Keep Constants, Add JSON
```csharp
// Old repository still works
services.AddScoped<IUserRepository, UserRepository>();

// New repository available for testing
// services.AddScoped<IUserRepository, UserRepositoryJsonBased>();
```

### Step 2: Test New Repository
Switch one repository at a time:
```csharp
services.AddScoped<IUserRepository, UserRepositoryJsonBased>();
// Test thoroughly
```

### Step 3: Migrate All Repositories
Once confident, migrate all repositories.

### Step 4: Remove Constants
Delete `StoredProcedureNames.cs` if no longer needed.

## 🛠️ Advanced Features

### Environment-Specific Configurations
```csharp
// In Program.cs
var env = builder.Environment;
builder.Configuration.AddJsonFile(
    $"storedProcedures.{env.EnvironmentName}.json",
    optional: true,
    reloadOnChange: true
);
```

### Hot Reload (Development Only)
```json
// appsettings.Development.json
{
  "StoredProcedures": {
    // Override for development
  }
}
```

### Validation Schema
Create JSON schema for IDE validation:
```json
{
  "$schema": "./storedProcedures.schema.json"
}
```

## 📈 Performance Considerations

- ✅ Configuration loaded **once** at startup (Singleton)
- ✅ Dictionary lookups are **O(1)**
- ✅ No reflection overhead
- ✅ Same ADO.NET performance as manual code
- ✅ Minimal memory footprint

## 🐛 Troubleshooting

### Procedure Not Found
```
InvalidOperationException: Stored procedure not found: User.GetById
```
**Solution**: Check JSON file for correct entity and operation names.

### Parameter Mismatch
```
SqlException: Procedure expects parameter '@UserId', which was not supplied.
```
**Solution**: Ensure parameter dictionary keys match JSON parameter names.

### JSON Not Copied
```
FileNotFoundException: Could not find file 'storedProcedures.json'
```
**Solution**: Verify `.csproj` has `CopyToOutputDirectory` set.

## 🎉 Summary

The JSON-based approach provides:
- ✅ **Maximum flexibility** - change procedures without recompiling
- ✅ **Self-documenting** - all info in one place
- ✅ **Environment-friendly** - different configs for different environments
- ✅ **Type-safe** - validated at runtime
- ✅ **Clean code** - shortest repository implementation
- ✅ **Easy maintenance** - non-developers can update JSON

**Recommendation**: Use this approach for **new projects** or when you need **maximum flexibility**.
