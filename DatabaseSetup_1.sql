-- =============================================
-- Admin Dashboard Database Setup Script
-- =============================================

USE master;
GO

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AdminDashboardDB')
BEGIN
    CREATE DATABASE AdminDashboardDB;
    PRINT 'Database AdminDashboardDB created successfully.';
END
GO

USE AdminDashboardDB;
GO

-- =============================================
-- Create Tables
-- =============================================

-- Users Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserId INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        FullName NVARCHAR(200) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        LastLoginDate DATETIME NULL,
        RoleId INT NOT NULL DEFAULT 1
    );
    PRINT 'Table Users created successfully.';
END
GO

-- Roles Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE Roles (
        RoleId INT IDENTITY(1,1) PRIMARY KEY,
        RoleName NVARCHAR(50) NOT NULL UNIQUE,
        Description NVARCHAR(255) NULL
    );
    PRINT 'Table Roles created successfully.';
END
GO

-- Add Foreign Key
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Roles')
BEGIN
    ALTER TABLE Users 
    ADD CONSTRAINT FK_Users_Roles 
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId);
END
GO

-- Insert Default Roles
IF NOT EXISTS (SELECT * FROM Roles WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO Roles (RoleName, Description) VALUES 
    ('Admin', 'Administrator with full access'),
    ('Manager', 'Manager with limited access'),
    ('User', 'Regular user with basic access');
    PRINT 'Default roles inserted successfully.';
END
GO

-- Insert Default Admin User (Password: Admin@123)
-- Password hash is for 'Admin@123' - you should use proper hashing in production
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, PasswordHash, Email, FullName, IsActive, RoleId)
    VALUES ('admin', 
            '$2a$11$8K1p/Z9jVWZ6Pm9aJQqPh.rZUvLEZKvHzKvGzp8aQj0/rGZ8FdPWa', 
            'admin@company.com', 
            'System Administrator', 
            1, 
            1);
    PRINT 'Default admin user created successfully.';
    PRINT 'Username: admin, Password: Admin@123';
END
GO

-- =============================================
-- Stored Procedures
-- =============================================

-- SP: Authenticate User
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_AuthenticateUser')
    DROP PROCEDURE SP_AuthenticateUser;
GO

CREATE PROCEDURE SP_AuthenticateUser
    @Username NVARCHAR(100),
    @PasswordHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserId,
        u.Username,
        u.Email,
        u.FullName,
        u.IsActive,
        u.RoleId,
        r.RoleName
    FROM Users u
    INNER JOIN Roles r ON u.RoleId = r.RoleId
    WHERE u.Username = @Username 
      AND u.PasswordHash = @PasswordHash
      AND u.IsActive = 1;
      
    -- Update last login date
    IF @@ROWCOUNT > 0
    BEGIN
        UPDATE Users 
        SET LastLoginDate = GETDATE()
        WHERE Username = @Username;
    END
END
GO

-- SP: Get User By Id
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetUserById')
    DROP PROCEDURE SP_GetUserById;
GO

CREATE PROCEDURE SP_GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserId,
        u.Username,
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

-- SP: Get All Users
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetAllUsers')
    DROP PROCEDURE SP_GetAllUsers;
GO

CREATE PROCEDURE SP_GetAllUsers
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserId,
        u.Username,
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

-- SP: Create User
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_CreateUser')
    DROP PROCEDURE SP_CreateUser;
GO

CREATE PROCEDURE SP_CreateUser
    @Username NVARCHAR(100),
    @PasswordHash NVARCHAR(255),
    @Email NVARCHAR(255),
    @FullName NVARCHAR(200),
    @RoleId INT,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO Users (Username, PasswordHash, Email, FullName, RoleId, IsActive)
        VALUES (@Username, @PasswordHash, @Email, @FullName, @RoleId, @IsActive);
        
        SELECT SCOPE_IDENTITY() AS UserId;
    END TRY
    BEGIN CATCH
        SELECT -1 AS UserId;
        THROW;
    END CATCH
END
GO

-- SP: Update User
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_UpdateUser')
    DROP PROCEDURE SP_UpdateUser;
GO

CREATE PROCEDURE SP_UpdateUser
    @UserId INT,
    @Username NVARCHAR(100),
    @Email NVARCHAR(255),
    @FullName NVARCHAR(200),
    @RoleId INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET 
        Username = @Username,
        Email = @Email,
        FullName = @FullName,
        RoleId = @RoleId,
        IsActive = @IsActive
    WHERE UserId = @UserId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- SP: Delete User
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_DeleteUser')
    DROP PROCEDURE SP_DeleteUser;
GO

CREATE PROCEDURE SP_DeleteUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM Users WHERE UserId = @UserId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- SP: Change Password
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_ChangePassword')
    DROP PROCEDURE SP_ChangePassword;
GO

CREATE PROCEDURE SP_ChangePassword
    @UserId INT,
    @NewPasswordHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET PasswordHash = @NewPasswordHash
    WHERE UserId = @UserId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- SP: Get All Roles
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetAllRoles')
    DROP PROCEDURE SP_GetAllRoles;
GO

CREATE PROCEDURE SP_GetAllRoles
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT RoleId, RoleName, Description
    FROM Roles
    ORDER BY RoleName;
END
GO

PRINT '========================================';
PRINT 'Database setup completed successfully!';
PRINT 'Database: AdminDashboardDB';
PRINT 'Default Username: admin';
PRINT 'Default Password: Admin@123';
PRINT '========================================';
GO
