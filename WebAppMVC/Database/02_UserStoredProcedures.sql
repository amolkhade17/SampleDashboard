-- =====================================================
-- Complete User Management Stored Procedures
-- ASP.NET Core 9 MVC Web Application
-- =====================================================

USE WebAppMVCDb;
GO

-- =====================================================
-- 1. USER AUTHENTICATION PROCEDURES
-- =====================================================

-- Procedure: Get User by Username
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetUserByUsername')
    DROP PROCEDURE sp_GetUserByUsername;
GO

CREATE PROCEDURE sp_GetUserByUsername
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        UserId,
        Username,
        Email,
        PasswordHash,
        FirstName,
        LastName,
        PhoneNumber,
        IsActive,
        IsEmailConfirmed,
        CreatedDate,
        LastLoginDate,
        LastModifiedDate,
        Role,
        ProfileImageUrl,
        Department,
        EmployeeId,
        CreatedBy,
        LastModifiedBy
    FROM Users 
    WHERE Username = @Username AND IsActive = 1;
END
GO

-- Procedure: Get User by Email
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetUserByEmail')
    DROP PROCEDURE sp_GetUserByEmail;
GO

CREATE PROCEDURE sp_GetUserByEmail
    @Email NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        UserId,
        Username,
        Email,
        PasswordHash,
        FirstName,
        LastName,
        PhoneNumber,
        IsActive,
        IsEmailConfirmed,
        CreatedDate,
        LastLoginDate,
        LastModifiedDate,
        Role,
        ProfileImageUrl,
        Department,
        EmployeeId,
        CreatedBy,
        LastModifiedBy
    FROM Users 
    WHERE Email = @Email AND IsActive = 1;
END
GO

-- Stored Procedure: Authenticate User
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_AuthenticateUser')
    DROP PROCEDURE sp_AuthenticateUser;
GO

CREATE PROCEDURE sp_AuthenticateUser
    @Username NVARCHAR(50),
    @Password NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- For demo purposes, this compares plain text passwords
    -- In production, you should implement proper password hashing
    SELECT 
        UserId,
        Username,
        Email,
        PasswordHash,
        FirstName,
        LastName,
        IsActive,
        CreatedDate,
        LastLoginDate,
        Role
    FROM Users 
    WHERE Username = @Username 
    AND IsActive = 1;
END
GO

-- Stored Procedure: Create User
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CreateUser')
    DROP PROCEDURE sp_CreateUser;
GO

CREATE PROCEDURE sp_CreateUser
    @Username NVARCHAR(50),
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(MAX),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @PhoneNumber NVARCHAR(20) = NULL,
    @Role NVARCHAR(20) = 'User',
    @Department NVARCHAR(100) = NULL,
    @EmployeeId NVARCHAR(50) = NULL,
    @CreatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @NewUserId INT;
    
    BEGIN TRY
        -- Check if username already exists
        IF EXISTS (SELECT 1 FROM Users WHERE Username = @Username)
        BEGIN
            RAISERROR('Username already exists', 16, 1);
            RETURN -1;
        END
        
        -- Check if email already exists
        IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
        BEGIN
            RAISERROR('Email already exists', 16, 1);
            RETURN -2;
        END
        
        INSERT INTO Users (
            Username, Email, PasswordHash, FirstName, LastName, 
            PhoneNumber, Role, Department, EmployeeId, 
            IsActive, IsEmailConfirmed, CreatedDate, CreatedBy
        )
        VALUES (
            @Username, @Email, @PasswordHash, @FirstName, @LastName,
            @PhoneNumber, @Role, @Department, @EmployeeId,
            1, 0, GETUTCDATE(), @CreatedBy
        );
        
        SET @NewUserId = SCOPE_IDENTITY();
        
        SELECT @NewUserId AS UserId;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- Stored Procedure: Update Last Login
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_UpdateLastLogin')
    DROP PROCEDURE sp_UpdateLastLogin;
GO

CREATE PROCEDURE sp_UpdateLastLogin
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users 
    SET LastLoginDate = GETUTCDATE()
    WHERE UserId = @UserId;
    
    RETURN @@ROWCOUNT;
END
GO

-- Stored Procedure: Check User Exists
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CheckUserExists')
    DROP PROCEDURE sp_CheckUserExists;
GO

CREATE PROCEDURE sp_CheckUserExists
    @Username NVARCHAR(50),
    @Email NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*)
    FROM Users 
    WHERE Username = @Username OR Email = @Email;
END
GO

-- =====================================================
-- 2. USER CRUD PROCEDURES
-- =====================================================

-- Procedure: Get All Users
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetAllUsers')
    DROP PROCEDURE sp_GetAllUsers;
GO

CREATE PROCEDURE sp_GetAllUsers
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        UserId,
        Username,
        Email,
        FirstName,
        LastName,
        PhoneNumber,
        IsActive,
        IsEmailConfirmed,
        CreatedDate,
        LastLoginDate,
        LastModifiedDate,
        Role,
        ProfileImageUrl,
        Department,
        EmployeeId,
        CreatedBy,
        LastModifiedBy
    FROM Users
    ORDER BY CreatedDate DESC;
END
GO

-- Procedure: Get User by ID
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetUserById')
    DROP PROCEDURE sp_GetUserById;
GO

CREATE PROCEDURE sp_GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        UserId,
        Username,
        Email,
        FirstName,
        LastName,
        PhoneNumber,
        IsActive,
        IsEmailConfirmed,
        CreatedDate,
        LastLoginDate,
        LastModifiedDate,
        Role,
        ProfileImageUrl,
        Department,
        EmployeeId,
        CreatedBy,
        LastModifiedBy
    FROM Users 
    WHERE UserId = @UserId;
END
GO

-- Procedure: Update User
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_UpdateUser')
    DROP PROCEDURE sp_UpdateUser;
GO

CREATE PROCEDURE sp_UpdateUser
    @UserId INT,
    @Username NVARCHAR(50),
    @Email NVARCHAR(100),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @PhoneNumber NVARCHAR(20) = NULL,
    @Role NVARCHAR(20),
    @Department NVARCHAR(100) = NULL,
    @EmployeeId NVARCHAR(50) = NULL,
    @IsActive BIT,
    @ModifiedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Check if user exists
        IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId)
        BEGIN
            RAISERROR('User not found', 16, 1);
            RETURN -1;
        END
        
        -- Check if username already exists for different user
        IF EXISTS (SELECT 1 FROM Users WHERE Username = @Username AND UserId != @UserId)
        BEGIN
            RAISERROR('Username already exists', 16, 1);
            RETURN -2;
        END
        
        -- Check if email already exists for different user
        IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email AND UserId != @UserId)
        BEGIN
            RAISERROR('Email already exists', 16, 1);
            RETURN -3;
        END
        
        -- Update user
        UPDATE Users 
        SET Username = @Username,
            Email = @Email,
            FirstName = @FirstName,
            LastName = @LastName,
            PhoneNumber = @PhoneNumber,
            Role = @Role,
            Department = @Department,
            EmployeeId = @EmployeeId,
            IsActive = @IsActive,
            LastModifiedDate = GETUTCDATE(),
            LastModifiedBy = @ModifiedBy
        WHERE UserId = @UserId;
        
        SELECT 1 AS Success;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- Procedure: Delete User (Soft Delete)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_DeleteUser')
    DROP PROCEDURE sp_DeleteUser;
GO

CREATE PROCEDURE sp_DeleteUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Check if user exists and is active
        IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId AND IsActive = 1)
        BEGIN
            RAISERROR('Active user not found', 16, 1);
            RETURN -1;
        END
        
        -- Soft delete user
        UPDATE Users 
        SET IsActive = 0
        WHERE UserId = @UserId;
        
        SELECT 1 AS Success;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

PRINT 'User authentication and CRUD stored procedures created successfully!';