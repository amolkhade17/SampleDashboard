-- =====================================================
-- MASTER DATABASE DEPLOYMENT SCRIPT
-- ASP.NET Core 9 MVC Web Application
-- Execute this script to set up the complete database
-- =====================================================

PRINT '==========================================================';
PRINT 'Starting database deployment for ASP.NET Core 9 MVC App';
PRINT 'Deployment started at: ' + CONVERT(NVARCHAR(30), GETDATE(), 120);
PRINT '==========================================================';
PRINT '';

-- =====================================================
-- STEP 1: DATABASE CREATION
-- =====================================================
PRINT 'STEP 1: Creating database...';
GO
:r "00_CreateDatabase.sql"
GO

-- =====================================================
-- STEP 2: TABLE CREATION
-- =====================================================
PRINT '';
PRINT 'STEP 2: Creating tables and indexes...';
GO
:r "01_CreateTables.sql"
GO

-- =====================================================
-- STEP 3: USER STORED PROCEDURES
-- =====================================================
PRINT '';
PRINT 'STEP 3: Creating user management stored procedures...';
GO
:r "02_UserStoredProcedures_Updated.sql"
GO

-- =====================================================
-- STEP 4: PRODUCT STORED PROCEDURES
-- =====================================================
PRINT '';
PRINT 'STEP 4: Creating product management stored procedures...';
GO
:r "03_ProductStoredProcedures.sql"
GO

-- =====================================================
-- STEP 5: APPLICATION STORED PROCEDURES
-- =====================================================
PRINT '';
PRINT 'STEP 5: Creating application and utility stored procedures...';
GO
:r "04_ApplicationStoredProcedures.sql"
GO

-- =====================================================
-- STEP 6: SAMPLE DATA
-- =====================================================
PRINT '';
PRINT 'STEP 6: Inserting sample data...';
GO
:r "05_SampleData.sql"
GO

-- =====================================================
-- STEP 7: POST-DEPLOYMENT VERIFICATION
-- =====================================================
PRINT '';
PRINT 'STEP 7: Running post-deployment verification...';

USE WebAppMVCDb;
GO

-- Verify all tables exist and have data
DECLARE @TableChecks TABLE (
    TableName NVARCHAR(50),
    Exists BIT,
    RecordCount INT
);

INSERT INTO @TableChecks (TableName, Exists, RecordCount)
SELECT 'Users', 
       CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users') THEN 1 ELSE 0 END,
       CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users') THEN (SELECT COUNT(*) FROM Users) ELSE 0 END
UNION ALL
SELECT 'Applications',
       CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Applications') THEN 1 ELSE 0 END,
       CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Applications') THEN (SELECT COUNT(*) FROM Applications) ELSE 0 END
UNION ALL
SELECT 'CrudOperations',
       CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CrudOperations') THEN 1 ELSE 0 END,
       CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CrudOperations') THEN (SELECT COUNT(*) FROM CrudOperations) ELSE 0 END
UNION ALL
SELECT 'Products',
       CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products') THEN 1 ELSE 0 END,
       CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products') THEN (SELECT COUNT(*) FROM Products) ELSE 0 END
UNION ALL
SELECT 'AuditLog',
       CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AuditLog') THEN 1 ELSE 0 END,
       CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AuditLog') THEN (SELECT COUNT(*) FROM AuditLog) ELSE 0 END
UNION ALL
SELECT 'UserSessions',
       CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserSessions') THEN 1 ELSE 0 END,
       CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserSessions') THEN (SELECT COUNT(*) FROM UserSessions) ELSE 0 END;

PRINT 'Table Verification Results:';
SELECT 
    TableName,
    CASE WHEN Exists = 1 THEN 'EXISTS' ELSE 'MISSING' END AS Status,
    RecordCount
FROM @TableChecks;

-- Verify stored procedures exist
DECLARE @ProcedureCount INT;
SELECT @ProcedureCount = COUNT(*)
FROM INFORMATION_SCHEMA.ROUTINES 
WHERE ROUTINE_TYPE = 'PROCEDURE' 
  AND ROUTINE_SCHEMA = 'dbo'
  AND ROUTINE_NAME LIKE 'sp_%';

PRINT '';
PRINT 'Stored Procedures Created: ' + CAST(@ProcedureCount AS NVARCHAR(10));

-- List all created stored procedures
PRINT '';
PRINT 'Available Stored Procedures:';
SELECT ROUTINE_NAME AS ProcedureName
FROM INFORMATION_SCHEMA.ROUTINES 
WHERE ROUTINE_TYPE = 'PROCEDURE' 
  AND ROUTINE_SCHEMA = 'dbo'
  AND ROUTINE_NAME LIKE 'sp_%'
ORDER BY ROUTINE_NAME;

-- Verify indexes exist
DECLARE @IndexCount INT;
SELECT @IndexCount = COUNT(*)
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.name IS NOT NULL 
  AND t.name IN ('Users', 'Applications', 'CrudOperations', 'Products', 'AuditLog', 'UserSessions');

PRINT '';
PRINT 'Database Indexes Created: ' + CAST(@IndexCount AS NVARCHAR(10));

-- Check foreign key constraints
DECLARE @FKCount INT;
SELECT @FKCount = COUNT(*)
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
WHERE CONSTRAINT_TYPE = 'FOREIGN KEY';

PRINT 'Foreign Key Constraints: ' + CAST(@FKCount AS NVARCHAR(10));

-- Test a sample stored procedure
PRINT '';
PRINT 'Testing sample stored procedure...';
EXEC sp_GetAllUsers @PageNumber = 1, @PageSize = 5;

-- =====================================================
-- DEPLOYMENT SUMMARY
-- =====================================================
PRINT '';
PRINT '==========================================================';
PRINT 'DATABASE DEPLOYMENT COMPLETED SUCCESSFULLY!';
PRINT 'Deployment finished at: ' + CONVERT(NVARCHAR(30), GETDATE(), 120);
PRINT '';
PRINT 'DEPLOYMENT SUMMARY:';
PRINT '- Database: WebAppMVCDb';
PRINT '- Tables: 6 (Users, Applications, CrudOperations, Products, AuditLog, UserSessions)';
PRINT '- Stored Procedures: ' + CAST(@ProcedureCount AS NVARCHAR(10));
PRINT '- Indexes: ' + CAST(@IndexCount AS NVARCHAR(10));
PRINT '- Foreign Keys: ' + CAST(@FKCount AS NVARCHAR(10));
PRINT '';
PRINT 'CONNECTION STRING (Update in appsettings.json):';
PRINT 'Server=(local);Database=WebAppMVCDb;Trusted_Connection=true;TrustServerCertificate=true;';
PRINT '';
PRINT 'SAMPLE LOGIN CREDENTIALS:';
PRINT 'Admin: admin / admin123';
PRINT 'Manager: manager1 / manager123';
PRINT 'User: user1 / user123';
PRINT 'Demo: demo / demo123';
PRINT '';
PRINT 'Ready to use with ASP.NET Core 9 MVC Application!';
PRINT '==========================================================';
GO