-- =============================================
-- Script: Fix Stored Procedures to Include PasswordHash
-- Issue: Login failing with "PasswordHash" error because stored procedures
--        don't return the PasswordHash column needed for authentication
-- Date: October 31, 2025
-- =============================================

USE AdminDashboardDB;
GO

PRINT 'Updating stored procedures to include PasswordHash column...';
GO

-- =============================================
-- Fix SP_GetAllUsers - Add PasswordHash
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetAllUsers')
    DROP PROCEDURE SP_GetAllUsers;
GO

PRINT 'Creating SP_GetAllUsers with PasswordHash...';
GO

CREATE PROCEDURE SP_GetAllUsers
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserId,
        u.Username,
        u.PasswordHash,      -- ADDED: Required for login authentication
        u.Email,
        u.FullName,
        u.IsActive,
        u.CreatedDate,
        u.LastLoginDate,
        u.RoleId,
        r.RoleName
    FROM Users u
    INNER JOIN Roles r ON u.RoleId = r.RoleId
    ORDER BY u.CreatedDate DESC;
END
GO

PRINT 'SP_GetAllUsers updated successfully.';
GO

-- =============================================
-- Fix SP_GetUserById - Add PasswordHash
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetUserById')
    DROP PROCEDURE SP_GetUserById;
GO

PRINT 'Creating SP_GetUserById with PasswordHash...';
GO

CREATE PROCEDURE SP_GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserId,
        u.Username,
        u.PasswordHash,      -- ADDED: Required for login authentication
        u.Email,
        u.FullName,
        u.IsActive,
        u.CreatedDate,
        u.LastLoginDate,
        u.RoleId,
        r.RoleName
    FROM Users u
    INNER JOIN Roles r ON u.RoleId = r.RoleId
    WHERE u.UserId = @UserId;
END
GO

PRINT 'SP_GetUserById updated successfully.';
GO

-- =============================================
-- Create SP_GetUserByUsername (if not exists)
-- This procedure is needed for login by username
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetUserByUsername')
    DROP PROCEDURE SP_GetUserByUsername;
GO

PRINT 'Creating SP_GetUserByUsername...';
GO

CREATE PROCEDURE SP_GetUserByUsername
    @Username NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserId,
        u.Username,
        u.PasswordHash,      -- REQUIRED: For password verification
        u.Email,
        u.FullName,
        u.IsActive,
        u.CreatedDate,
        u.LastLoginDate,
        u.RoleId,
        r.RoleName
    FROM Users u
    INNER JOIN Roles r ON u.RoleId = r.RoleId
    WHERE u.Username = @Username;
END
GO

PRINT 'SP_GetUserByUsername created successfully.';
GO

-- =============================================
-- Verify the changes
-- =============================================
PRINT '';
PRINT 'Verification:';
PRINT '=============';

-- Check if stored procedures exist
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetAllUsers')
    PRINT '✓ SP_GetAllUsers exists';
ELSE
    PRINT '✗ SP_GetAllUsers missing!';

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetUserById')
    PRINT '✓ SP_GetUserById exists';
ELSE
    PRINT '✗ SP_GetUserById missing!';

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetUserByUsername')
    PRINT '✓ SP_GetUserByUsername exists';
ELSE
    PRINT '✗ SP_GetUserByUsername missing!';

-- Test the procedures
PRINT '';
PRINT 'Testing SP_GetAllUsers:';
EXEC SP_GetAllUsers;

PRINT '';
PRINT 'All stored procedures have been updated successfully!';
PRINT 'You can now test the login functionality.';
GO
