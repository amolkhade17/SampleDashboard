# Generic Database Helper - Implementation Guide

## Overview
This implementation provides a **generic, reusable database helper class** (`DbHelper`) that eliminates code duplication across repositories. All stored procedure names are centralized in `StoredProcedureNames` class.

## Architecture Benefits

### 1. **Code Reduction**
- **Before**: Each repository had ~500 lines of repetitive ADO.NET code
- **After**: Each repository has ~150 lines of clean, readable code
- **Reduction**: ~70% less code per repository

### 2. **Maintainability**
- All stored procedure names in one place
- Easy to update connection logic globally
- Single point of failure = easier debugging

### 3. **Consistency**
- All repositories use the same patterns
- Standardized error handling
- Uniform parameter creation

## File Structure

```
AdminDashboard.Infrastructure/
├── Data/
│   ├── DbHelper.cs                          # Generic database operations
│   ├── StoredProcedureNames.cs              # All SP name mappings
│   └── DbConnectionFactory.cs               # (existing)
├── Repositories/
│   ├── UserRepositoryRefactored.cs          # Example implementation
│   ├── ProductRepositoryRefactored.cs       # Example implementation
│   └── MakerCheckerRepositoryRefactored.cs  # Example implementation
```

## Key Components

### 1. DbHelper.cs
Generic helper with the following methods:

- `ExecuteStoredProcedureSingleAsync<T>()` - Returns single entity
- `ExecuteStoredProcedureListAsync<T>()` - Returns list of entities
- `ExecuteStoredProcedureNonQueryAsync()` - Returns affected row count
- `ExecuteStoredProcedureWithOutputAsync<T>()` - Returns output parameter value
- `ExecuteStoredProcedureScalarAsync<T>()` - Returns single value
- `ExecuteStoredProcedureDynamicAsync()` - Returns dynamic results (reports)
- `ExecuteInTransactionAsync<T>()` - Execute within transaction

### 2. StoredProcedureNames.cs
Centralized stored procedure name constants organized by entity:

```csharp
StoredProcedureNames.User.Authenticate
StoredProcedureNames.User.GetById
StoredProcedureNames.Product.GetAll
StoredProcedureNames.Product.Create
StoredProcedureNames.MakerChecker.ApproveRecord
// etc...
```

## Usage Examples

### Example 1: Get Single Entity
```csharp
public async Task<User?> GetByIdAsync(int userId)
{
    return await _dbHelper.ExecuteStoredProcedureSingleAsync(
        StoredProcedureNames.User.GetById,
        MapUserFromReader,
        DbHelper.CreateParameter("@UserId", userId)
    );
}
```

### Example 2: Get List of Entities
```csharp
public async Task<IEnumerable<Product>> GetAllAsync()
{
    return await _dbHelper.ExecuteStoredProcedureListAsync(
        StoredProcedureNames.Product.GetAll,
        MapProductFromReader
    );
}
```

### Example 3: Create with Output Parameter
```csharp
public async Task<int> CreateAsync(User user)
{
    return await _dbHelper.ExecuteStoredProcedureWithOutputAsync<int>(
        StoredProcedureNames.User.Create,
        "@UserId",
        System.Data.SqlDbType.Int,
        DbHelper.CreateParameter("@Username", user.Username),
        DbHelper.CreateParameter("@Email", user.Email),
        DbHelper.CreateParameter("@FullName", user.FullName)
    );
}
```

### Example 4: Update/Delete
```csharp
public async Task<bool> DeleteAsync(int userId)
{
    var rowsAffected = await _dbHelper.ExecuteStoredProcedureNonQueryAsync(
        StoredProcedureNames.User.Delete,
        DbHelper.CreateParameter("@UserId", userId)
    );
    
    return rowsAffected > 0;
}
```

### Example 5: Transaction
```csharp
public async Task<bool> ComplexOperationAsync()
{
    return await _dbHelper.ExecuteInTransactionAsync(async (conn, trans) =>
    {
        // Multiple operations here
        return true;
    });
}
```

## Migration Guide

### Step 1: Add New Files
✅ Created:
- `DbHelper.cs`
- `StoredProcedureNames.cs`
- `*RepositoryRefactored.cs` examples

### Step 2: Register DbHelper in DI
```csharp
// In DependencyInjection.cs
services.AddScoped<DbHelper>();
```

### Step 3: Update Repositories (One by One)
1. Inject `DbHelper` instead of `DbConnectionFactory`
2. Replace manual ADO.NET code with `DbHelper` methods
3. Use `StoredProcedureNames` constants
4. Keep mapper methods unchanged

### Step 4: Test Thoroughly
- Test each repository after refactoring
- Verify all CRUD operations
- Check error handling

## Code Comparison

### BEFORE (Traditional Approach)
```csharp
public async Task<User?> GetByIdAsync(int userId)
{
    using var connection = _connectionFactory.CreateConnection();
    using var command = connection.CreateCommand();
    
    command.CommandText = "SP_GetUserById";  // Hard-coded string
    command.CommandType = CommandType.StoredProcedure;
    
    command.Parameters.Add(new SqlParameter("@UserId", userId));
    
    connection.Open();
    using var reader = await ((SqlCommand)command).ExecuteReaderAsync();
    
    if (await reader.ReadAsync())
    {
        return MapUserFromReader(reader);
    }
    
    return null;
}
```

### AFTER (Generic Helper Approach)
```csharp
public async Task<User?> GetByIdAsync(int userId)
{
    return await _dbHelper.ExecuteStoredProcedureSingleAsync(
        StoredProcedureNames.User.GetById,  // Centralized constant
        MapUserFromReader,
        DbHelper.CreateParameter("@UserId", userId)
    );
}
```

**Benefits**:
- 15 lines reduced to 6 lines
- No manual connection management
- Centralized procedure name
- Cleaner, more readable code

## Adding New Stored Procedures

### 1. Add to StoredProcedureNames.cs
```csharp
public static class User
{
    // Existing procedures...
    public const string GetUsersByRole = "SP_GetUsersByRole";  // NEW
}
```

### 2. Use in Repository
```csharp
public async Task<IEnumerable<User>> GetByRoleAsync(int roleId)
{
    return await _dbHelper.ExecuteStoredProcedureListAsync(
        StoredProcedureNames.User.GetUsersByRole,
        MapUserFromReader,
        DbHelper.CreateParameter("@RoleId", roleId)
    );
}
```

## Best Practices

### 1. Mapper Methods
- Keep mappers private and static
- Reuse across all methods in repository
- Handle DBNull values properly

```csharp
private static User MapUserFromReader(SqlDataReader reader)
{
    return new User
    {
        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
        Email = reader.IsDBNull(reader.GetOrdinal("Email")) 
            ? null 
            : reader.GetString(reader.GetOrdinal("Email"))
    };
}
```

### 2. Parameter Naming
- Use consistent naming: `@ParameterName`
- Match stored procedure parameter names exactly

### 3. Error Handling
- Let exceptions bubble up
- DbHelper handles connection disposal
- Add try-catch only in service layer

### 4. Null Handling
```csharp
DbHelper.CreateParameter("@Email", email ?? DBNull.Value)
// OR use the helper which handles nulls automatically
DbHelper.CreateParameter("@Email", email)
```

## Performance Considerations

1. **Connection Pooling**: Automatically handled by ADO.NET
2. **Async/Await**: All methods are async for scalability
3. **Disposal**: Using statements ensure proper cleanup
4. **Memory**: DataReader streams results (low memory footprint)

## Testing

```csharp
[Fact]
public async Task GetByIdAsync_ShouldReturnUser()
{
    // Arrange
    var dbHelper = new DbHelper(configuration);
    var repository = new UserRepositoryRefactored(dbHelper);
    
    // Act
    var user = await repository.GetByIdAsync(1);
    
    // Assert
    Assert.NotNull(user);
    Assert.Equal(1, user.UserId);
}
```

## Rollback Plan

If issues arise:
1. Keep old repositories (`UserRepository.cs`) intact
2. Use refactored versions in parallel
3. Switch DI registration to revert:

```csharp
// services.AddScoped<IUserRepository, UserRepositoryRefactored>(); // NEW
services.AddScoped<IUserRepository, UserRepository>(); // OLD (fallback)
```

## Future Enhancements

1. **Dapper Integration** (Optional)
   - Replace ADO.NET with Dapper for even less code
   - Keep stored procedure approach

2. **Caching Layer**
   - Add Redis/Memory cache
   - Cache frequently accessed data

3. **Logging**
   - Log all SP calls
   - Track execution times

4. **Retry Logic**
   - Add Polly for transient fault handling
   - Automatic retries for deadlocks

## Conclusion

This generic approach provides:
- ✅ 70% less repository code
- ✅ Single point of maintenance
- ✅ Consistent patterns across all repositories
- ✅ Easy to test and debug
- ✅ Scalable for future growth

**Recommendation**: Migrate repositories gradually, starting with the least complex ones (Category, Role) and working up to complex ones (User, Product, MakerChecker).
