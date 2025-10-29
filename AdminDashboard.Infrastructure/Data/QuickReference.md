# Quick Reference: DbHelper & StoredProcedureNames

## Import Statements
```csharp
using AdminDashboard.Infrastructure.Data;
using System.Data.SqlClient;
```

## Constructor Injection
```csharp
private readonly DbHelper _dbHelper;

public YourRepository(DbHelper dbHelper)
{
    _dbHelper = dbHelper;
}
```

## Common Patterns

### 1️⃣ Get Single Entity
```csharp
return await _dbHelper.ExecuteStoredProcedureSingleAsync(
    StoredProcedureNames.Entity.GetById,
    MapEntityFromReader,
    DbHelper.CreateParameter("@Id", id)
);
```

### 2️⃣ Get List of Entities
```csharp
return await _dbHelper.ExecuteStoredProcedureListAsync(
    StoredProcedureNames.Entity.GetAll,
    MapEntityFromReader
);
```

### 3️⃣ Get List with Filter
```csharp
return await _dbHelper.ExecuteStoredProcedureListAsync(
    StoredProcedureNames.Entity.GetByStatus,
    MapEntityFromReader,
    DbHelper.CreateParameter("@Status", status)
);
```

### 4️⃣ Create (with Output Parameter)
```csharp
return await _dbHelper.ExecuteStoredProcedureWithOutputAsync<int>(
    StoredProcedureNames.Entity.Create,
    "@NewId",
    System.Data.SqlDbType.Int,
    DbHelper.CreateParameter("@Name", entity.Name),
    DbHelper.CreateParameter("@Value", entity.Value)
);
```

### 5️⃣ Update (returns bool)
```csharp
var rowsAffected = await _dbHelper.ExecuteStoredProcedureNonQueryAsync(
    StoredProcedureNames.Entity.Update,
    DbHelper.CreateParameter("@Id", entity.Id),
    DbHelper.CreateParameter("@Name", entity.Name)
);

return rowsAffected > 0;
```

### 6️⃣ Delete (returns bool)
```csharp
var rowsAffected = await _dbHelper.ExecuteStoredProcedureNonQueryAsync(
    StoredProcedureNames.Entity.Delete,
    DbHelper.CreateParameter("@Id", id)
);

return rowsAffected > 0;
```

### 7️⃣ Get Scalar Value
```csharp
return await _dbHelper.ExecuteStoredProcedureScalarAsync<int>(
    StoredProcedureNames.Entity.GetCount,
    DbHelper.CreateParameter("@Status", status)
);
```

### 8️⃣ Dynamic Results (for Reports)
```csharp
return await _dbHelper.ExecuteStoredProcedureDynamicAsync(
    StoredProcedureNames.Report.GetSummary,
    DbHelper.CreateParameter("@StartDate", startDate),
    DbHelper.CreateParameter("@EndDate", endDate)
);
```

## Available Stored Procedures

### User Operations
```csharp
StoredProcedureNames.User.Authenticate
StoredProcedureNames.User.GetById
StoredProcedureNames.User.GetByUsername
StoredProcedureNames.User.GetAll
StoredProcedureNames.User.Create
StoredProcedureNames.User.Update
StoredProcedureNames.User.Delete
StoredProcedureNames.User.UpdateStatus
```

### Product Operations
```csharp
StoredProcedureNames.Product.GetById
StoredProcedureNames.Product.GetAll
StoredProcedureNames.Product.Create
StoredProcedureNames.Product.Update
StoredProcedureNames.Product.Delete
StoredProcedureNames.Product.UpdateStock
```

### Category Operations
```csharp
StoredProcedureNames.Category.GetAll
StoredProcedureNames.Category.Create
```

### MakerChecker Operations
```csharp
StoredProcedureNames.MakerChecker.GetPendingRecords
StoredProcedureNames.MakerChecker.GetApprovedRecords
StoredProcedureNames.MakerChecker.GetRejectedRecords
StoredProcedureNames.MakerChecker.GetRecordById
StoredProcedureNames.MakerChecker.CreatePendingRecord
StoredProcedureNames.MakerChecker.ApproveRecord
StoredProcedureNames.MakerChecker.RejectRecord
```

### Dashboard Operations
```csharp
StoredProcedureNames.Dashboard.GetCounts
StoredProcedureNames.Dashboard.GetRecentActivities
```

### Report Operations
```csharp
StoredProcedureNames.Report.GetUserActivity
StoredProcedureNames.Report.GetProductStock
StoredProcedureNames.Report.GetMakerCheckerSummary
```

### Role Operations
```csharp
StoredProcedureNames.Role.GetAll
```

## Parameter Helpers

### Create Parameter
```csharp
DbHelper.CreateParameter("@Name", value)
DbHelper.CreateParameter("@Name", value, SqlDbType.VarChar)
```

### Create Output Parameter
```csharp
DbHelper.CreateOutputParameter("@NewId", SqlDbType.Int)
DbHelper.CreateOutputParameter("@Message", SqlDbType.VarChar, 100)
```

## Mapper Template

```csharp
private static Entity MapEntityFromReader(SqlDataReader reader)
{
    return new Entity
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Name = reader.GetString(reader.GetOrdinal("Name")),
        
        // Nullable string
        Description = reader.IsDBNull(reader.GetOrdinal("Description"))
            ? null
            : reader.GetString(reader.GetOrdinal("Description")),
            
        // Nullable int
        CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId"))
            ? null
            : reader.GetInt32(reader.GetOrdinal("CategoryId")),
            
        // Nullable DateTime
        ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate"))
            ? null
            : reader.GetDateTime(reader.GetOrdinal("ModifiedDate"))
    };
}
```

## Complete Repository Example

```csharp
using System.Data.SqlClient;
using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;
using AdminDashboard.Infrastructure.Data;

namespace AdminDashboard.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly DbHelper _dbHelper;

    public CategoryRepository(DbHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _dbHelper.ExecuteStoredProcedureListAsync(
            StoredProcedureNames.Category.GetAll,
            MapCategoryFromReader
        );
    }

    public async Task<int> CreateAsync(Category category)
    {
        return await _dbHelper.ExecuteStoredProcedureWithOutputAsync<int>(
            StoredProcedureNames.Category.Create,
            "@CategoryId",
            System.Data.SqlDbType.Int,
            DbHelper.CreateParameter("@CategoryName", category.CategoryName)
        );
    }

    private static Category MapCategoryFromReader(SqlDataReader reader)
    {
        return new Category
        {
            CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
            CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
        };
    }
}
```

## Adding New Stored Procedure

### Step 1: Add to StoredProcedureNames.cs
```csharp
public static class Entity
{
    // Existing...
    public const string GetByStatus = "SP_GetEntityByStatus";
}
```

### Step 2: Use in Repository
```csharp
public async Task<IEnumerable<Entity>> GetByStatusAsync(string status)
{
    return await _dbHelper.ExecuteStoredProcedureListAsync(
        StoredProcedureNames.Entity.GetByStatus,
        MapEntityFromReader,
        DbHelper.CreateParameter("@Status", status)
    );
}
```

## Error Handling

DbHelper automatically handles:
- ✅ Connection opening/closing
- ✅ Command disposal
- ✅ Reader disposal
- ✅ Parameter null handling

You handle:
- ❌ Business logic validation
- ❌ Try-catch in service layer (if needed)

## Tips

1. **Always use constants** from `StoredProcedureNames` - never hard-code SP names
2. **Keep mappers simple** - one per entity type
3. **Test incrementally** - one method at a time
4. **Use meaningful parameter names** - match database exactly
5. **Handle nulls properly** - check `IsDBNull()` before reading

## Performance

- All operations are **async** for scalability
- Connection pooling is **automatic**
- Memory efficient with **DataReader streaming**
- No ORM overhead - **direct ADO.NET** performance
