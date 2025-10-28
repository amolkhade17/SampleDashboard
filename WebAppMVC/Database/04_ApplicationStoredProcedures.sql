-- =====================================================
-- Application Management Stored Procedures
-- ASP.NET Core 9 MVC Web Application
-- =====================================================

USE WebAppMVCDb;
GO

-- =====================================================
-- 1. APPLICATION MANAGEMENT PROCEDURES
-- =====================================================

-- Procedure: Get All Applications
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetAllApplications')
    DROP PROCEDURE sp_GetAllApplications;
GO

CREATE PROCEDURE sp_GetAllApplications
    @UserRole NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ApplicationId,
        ApplicationName,
        Description,
        IconClass,
        BackgroundColor,
        RouteUrl,
        IsActive,
        SortOrder,
        RequiredRole,
        CreatedDate,
        CreatedBy,
        LastModifiedDate,
        LastModifiedBy,
        Version
    FROM Applications 
    WHERE IsActive = 1 
          AND (@UserRole IS NULL OR RequiredRole IS NULL OR RequiredRole = @UserRole)
    ORDER BY SortOrder, ApplicationName;
END
GO

-- Procedure: Get Application by ID
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetApplicationById')
    DROP PROCEDURE sp_GetApplicationById;
GO

CREATE PROCEDURE sp_GetApplicationById
    @ApplicationId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ApplicationId,
        ApplicationName,
        Description,
        IconClass,
        BackgroundColor,
        RouteUrl,
        IsActive,
        SortOrder,
        RequiredRole,
        CreatedDate,
        CreatedBy,
        LastModifiedDate,
        LastModifiedBy,
        Version
    FROM Applications 
    WHERE ApplicationId = @ApplicationId;
END
GO

-- Procedure: Get CRUD Operations by Application ID
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetCrudOperationsByApplicationId')
    DROP PROCEDURE sp_GetCrudOperationsByApplicationId;
GO

CREATE PROCEDURE sp_GetCrudOperationsByApplicationId
    @ApplicationId INT,
    @UserRole NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        co.OperationId,
        co.ApplicationId,
        co.OperationName,
        co.Description,
        co.IconClass,
        co.BackgroundColor,
        co.ActionName,
        co.ControllerName,
        co.RequiredRole,
        co.IsActive,
        co.SortOrder,
        co.CreatedDate,
        a.ApplicationName
    FROM CrudOperations co
    INNER JOIN Applications a ON co.ApplicationId = a.ApplicationId
    WHERE co.ApplicationId = @ApplicationId 
          AND co.IsActive = 1
          AND a.IsActive = 1
          AND (@UserRole IS NULL OR co.RequiredRole IS NULL OR co.RequiredRole = @UserRole)
    ORDER BY co.SortOrder, co.OperationName;
END
GO

-- Procedure: Get All CRUD Operations
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetAllCrudOperations')
    DROP PROCEDURE sp_GetAllCrudOperations;
GO

CREATE PROCEDURE sp_GetAllCrudOperations
    @UserRole NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        co.OperationId,
        co.ApplicationId,
        co.OperationName,
        co.Description,
        co.IconClass,
        co.BackgroundColor,
        co.ActionName,
        co.ControllerName,
        co.RequiredRole,
        co.IsActive,
        co.SortOrder,
        co.CreatedDate,
        a.ApplicationName
    FROM CrudOperations co
    INNER JOIN Applications a ON co.ApplicationId = a.ApplicationId
    WHERE co.IsActive = 1
          AND a.IsActive = 1
          AND (@UserRole IS NULL OR co.RequiredRole IS NULL OR co.RequiredRole = @UserRole)
    ORDER BY a.SortOrder, a.ApplicationName, co.SortOrder, co.OperationName;
END
GO

-- =====================================================
-- 2. AUDIT AND REPORTING PROCEDURES
-- =====================================================

-- Procedure: Get Audit Log
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetAuditLog')
    DROP PROCEDURE sp_GetAuditLog;
GO

CREATE PROCEDURE sp_GetAuditLog
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @TableName NVARCHAR(100) = NULL,
    @Action NVARCHAR(20) = NULL,
    @UserId INT = NULL,
    @StartDate DATETIME2(7) = NULL,
    @EndDate DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Get total count
    DECLARE @TotalCount INT;
    
    SELECT @TotalCount = COUNT(*)
    FROM AuditLog a
    WHERE (@TableName IS NULL OR a.TableName = @TableName)
      AND (@Action IS NULL OR a.Action = @Action)
      AND (@UserId IS NULL OR a.UserId = @UserId)
      AND (@StartDate IS NULL OR a.Timestamp >= @StartDate)
      AND (@EndDate IS NULL OR a.Timestamp <= @EndDate);
    
    -- Get paged results
    SELECT 
        a.AuditId,
        a.TableName,
        a.RecordId,
        a.Action,
        a.OldValues,
        a.NewValues,
        a.UserId,
        a.Username,
        a.Timestamp,
        a.IPAddress,
        a.UserAgent,
        @TotalCount as TotalCount
    FROM AuditLog a
    WHERE (@TableName IS NULL OR a.TableName = @TableName)
      AND (@Action IS NULL OR a.Action = @Action)
      AND (@UserId IS NULL OR a.UserId = @UserId)
      AND (@StartDate IS NULL OR a.Timestamp >= @StartDate)
      AND (@EndDate IS NULL OR a.Timestamp <= @EndDate)
    ORDER BY a.Timestamp DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Procedure: Get User Activity Summary
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetUserActivitySummary')
    DROP PROCEDURE sp_GetUserActivitySummary;
GO

CREATE PROCEDURE sp_GetUserActivitySummary
    @StartDate DATETIME2(7) = NULL,
    @EndDate DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @StartDate IS NULL
        SET @StartDate = DATEADD(DAY, -30, GETUTCDATE());
        
    IF @EndDate IS NULL
        SET @EndDate = GETUTCDATE();
    
    SELECT 
        u.UserId,
        u.Username,
        u.FirstName,
        u.LastName,
        u.Role,
        COUNT(a.AuditId) AS TotalActions,
        COUNT(CASE WHEN a.Action = 'INSERT' THEN 1 END) AS CreateActions,
        COUNT(CASE WHEN a.Action = 'UPDATE' THEN 1 END) AS UpdateActions,
        COUNT(CASE WHEN a.Action = 'DELETE' THEN 1 END) AS DeleteActions,
        MAX(a.Timestamp) AS LastActivity,
        u.LastLoginDate
    FROM Users u
    LEFT JOIN AuditLog a ON u.UserId = a.UserId 
        AND a.Timestamp >= @StartDate 
        AND a.Timestamp <= @EndDate
    WHERE u.IsActive = 1
    GROUP BY u.UserId, u.Username, u.FirstName, u.LastName, u.Role, u.LastLoginDate
    ORDER BY COUNT(a.AuditId) DESC, u.LastLoginDate DESC;
END
GO

-- Procedure: Get System Statistics
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetSystemStatistics')
    DROP PROCEDURE sp_GetSystemStatistics;
GO

CREATE PROCEDURE sp_GetSystemStatistics
AS
BEGIN
    SET NOCOUNT ON;
    
    -- User Statistics
    SELECT 
        'Users' AS Category,
        COUNT(*) AS Total,
        COUNT(CASE WHEN IsActive = 1 THEN 1 END) AS Active,
        COUNT(CASE WHEN Role = 'Admin' THEN 1 END) AS Admins,
        COUNT(CASE WHEN Role = 'Manager' THEN 1 END) AS Managers,
        COUNT(CASE WHEN Role = 'User' THEN 1 END) AS RegularUsers,
        COUNT(CASE WHEN LastLoginDate >= DATEADD(DAY, -7, GETUTCDATE()) THEN 1 END) AS ActiveThisWeek
    FROM Users
    
    UNION ALL
    
    -- Product Statistics
    SELECT 
        'Products' AS Category,
        COUNT(*) AS Total,
        COUNT(CASE WHEN IsActive = 1 THEN 1 END) AS Active,
        COUNT(CASE WHEN Quantity = 0 THEN 1 END) AS OutOfStock,
        COUNT(CASE WHEN Quantity <= MinimumStock THEN 1 END) AS LowStock,
        COUNT(DISTINCT Category) AS Categories,
        COUNT(DISTINCT Brand) AS Brands
    FROM Products
    
    UNION ALL
    
    -- Application Statistics
    SELECT 
        'Applications' AS Category,
        COUNT(*) AS Total,
        COUNT(CASE WHEN IsActive = 1 THEN 1 END) AS Active,
        0 AS Col3,
        0 AS Col4,
        0 AS Col5,
        0 AS Col6
    FROM Applications
    
    UNION ALL
    
    -- Session Statistics
    SELECT 
        'Sessions' AS Category,
        COUNT(*) AS Total,
        COUNT(CASE WHEN IsActive = 1 AND ExpiryDate > GETUTCDATE() THEN 1 END) AS Active,
        COUNT(CASE WHEN ExpiryDate <= GETUTCDATE() THEN 1 END) AS Expired,
        COUNT(CASE WHEN CreatedDate >= DATEADD(DAY, -1, GETUTCDATE()) THEN 1 END) AS CreatedToday,
        0 AS Col5,
        0 AS Col6
    FROM UserSessions;
END
GO

-- =====================================================
-- 3. UTILITY PROCEDURES
-- =====================================================

-- Procedure: Search All Tables
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GlobalSearch')
    DROP PROCEDURE sp_GlobalSearch;
GO

CREATE PROCEDURE sp_GlobalSearch
    @SearchTerm NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Search Users
    SELECT 
        'Users' AS TableName,
        CAST(UserId AS NVARCHAR(50)) AS RecordId,
        Username + ' (' + FirstName + ' ' + LastName + ')' AS DisplayText,
        'User' AS RecordType,
        CreatedDate AS RecordDate
    FROM Users 
    WHERE IsActive = 1 
      AND (Username LIKE '%' + @SearchTerm + '%' 
           OR FirstName LIKE '%' + @SearchTerm + '%' 
           OR LastName LIKE '%' + @SearchTerm + '%' 
           OR Email LIKE '%' + @SearchTerm + '%')
    
    UNION ALL
    
    -- Search Products
    SELECT 
        'Products' AS TableName,
        CAST(ProductId AS NVARCHAR(50)) AS RecordId,
        ProductName + ' (' + ProductCode + ')' AS DisplayText,
        'Product' AS RecordType,
        CreatedDate AS RecordDate
    FROM Products 
    WHERE IsActive = 1 
      AND (ProductName LIKE '%' + @SearchTerm + '%' 
           OR ProductCode LIKE '%' + @SearchTerm + '%' 
           OR Description LIKE '%' + @SearchTerm + '%'
           OR Brand LIKE '%' + @SearchTerm + '%'
           OR SKU LIKE '%' + @SearchTerm + '%')
    
    UNION ALL
    
    -- Search Applications
    SELECT 
        'Applications' AS TableName,
        CAST(ApplicationId AS NVARCHAR(50)) AS RecordId,
        ApplicationName AS DisplayText,
        'Application' AS RecordType,
        CreatedDate AS RecordDate
    FROM Applications 
    WHERE IsActive = 1 
      AND (ApplicationName LIKE '%' + @SearchTerm + '%' 
           OR Description LIKE '%' + @SearchTerm + '%')
    
    ORDER BY RecordDate DESC;
END
GO

-- Procedure: Database Health Check
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_DatabaseHealthCheck')
    DROP PROCEDURE sp_DatabaseHealthCheck;
GO

CREATE PROCEDURE sp_DatabaseHealthCheck
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Table sizes and record counts
    SELECT 
        t.name AS TableName,
        p.rows AS RecordCount,
        CAST(ROUND((SUM(a.used_pages) * 8) / 1024.0, 2) AS DECIMAL(10,2)) AS UsedSpaceMB,
        CAST(ROUND((SUM(a.total_pages) * 8) / 1024.0, 2) AS DECIMAL(10,2)) AS TotalSpaceMB
    FROM sys.tables t
    INNER JOIN sys.indexes i ON t.object_id = i.object_id
    INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
    INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
    WHERE t.name IN ('Users', 'Products', 'Applications', 'CrudOperations', 'AuditLog', 'UserSessions')
    GROUP BY t.name, p.rows
    ORDER BY UsedSpaceMB DESC;
    
    -- Index information
    SELECT 
        t.name AS TableName,
        i.name AS IndexName,
        i.type_desc AS IndexType,
        CASE WHEN i.is_primary_key = 1 THEN 'Primary Key'
             WHEN i.is_unique = 1 THEN 'Unique'
             ELSE 'Non-Unique' END AS KeyType
    FROM sys.tables t
    INNER JOIN sys.indexes i ON t.object_id = i.object_id
    WHERE t.name IN ('Users', 'Products', 'Applications', 'CrudOperations', 'AuditLog', 'UserSessions')
      AND i.name IS NOT NULL
    ORDER BY t.name, i.name;
    
    -- Constraint information
    SELECT 
        t.name AS TableName,
        c.name AS ConstraintName,
        c.type_desc AS ConstraintType
    FROM sys.tables t
    INNER JOIN sys.check_constraints c ON t.object_id = c.parent_object_id
    WHERE t.name IN ('Users', 'Products', 'Applications', 'CrudOperations', 'AuditLog', 'UserSessions')
    
    UNION ALL
    
    SELECT 
        t.name AS TableName,
        fk.name AS ConstraintName,
        'FOREIGN KEY' AS ConstraintType
    FROM sys.tables t
    INNER JOIN sys.foreign_keys fk ON t.object_id = fk.parent_object_id
    WHERE t.name IN ('Users', 'Products', 'Applications', 'CrudOperations', 'AuditLog', 'UserSessions')
    
    ORDER BY TableName, ConstraintName;
END
GO

PRINT 'Application management and utility stored procedures created successfully!';
PRINT 'Procedures: Application management, Audit trails, System statistics, Utilities';
GO