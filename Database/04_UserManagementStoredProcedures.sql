-- Additional User Management Stored Procedures
USE WebAppMVCDb;
GO

-- Get All Users
IF OBJECT_ID('sp_GetAllUsers', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetAllUsers;
GO

CREATE PROCEDURE sp_GetAllUsers
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT UserId, Username, Email, PasswordHash, FirstName, LastName, 
           PhoneNumber, IsActive, IsEmailConfirmed, CreatedDate, 
           LastLoginDate, LastModifiedDate, Role, ProfileImageUrl, 
           Department, EmployeeId, CreatedBy, LastModifiedBy
    FROM Users 
    ORDER BY CreatedDate DESC;
END
GO

-- Get User By Id
IF OBJECT_ID('sp_GetUserById', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetUserById;
GO

CREATE PROCEDURE sp_GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT UserId, Username, Email, PasswordHash, FirstName, LastName, 
           PhoneNumber, IsActive, IsEmailConfirmed, CreatedDate, 
           LastLoginDate, LastModifiedDate, Role, ProfileImageUrl, 
           Department, EmployeeId, CreatedBy, LastModifiedBy
    FROM Users 
    WHERE UserId = @UserId;
END
GO

-- Update User (Enhanced)
IF OBJECT_ID('sp_UpdateUser', 'P') IS NOT NULL
    DROP PROCEDURE sp_UpdateUser;
GO

CREATE PROCEDURE sp_UpdateUser
    @UserId INT,
    @Username NVARCHAR(50),
    @Email NVARCHAR(100),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @PhoneNumber NVARCHAR(20) = NULL,
    @IsActive BIT,
    @IsEmailConfirmed BIT,
    @Role NVARCHAR(20),
    @ProfileImageUrl NVARCHAR(500) = NULL,
    @Department NVARCHAR(100) = NULL,
    @EmployeeId NVARCHAR(50) = NULL,
    @LastModifiedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users 
    SET Username = @Username,
        Email = @Email,
        FirstName = @FirstName,
        LastName = @LastName,
        PhoneNumber = @PhoneNumber,
        IsActive = @IsActive,
        IsEmailConfirmed = @IsEmailConfirmed,
        Role = @Role,
        ProfileImageUrl = @ProfileImageUrl,
        Department = @Department,
        EmployeeId = @EmployeeId,
        LastModifiedDate = GETDATE(),
        LastModifiedBy = @LastModifiedBy
    WHERE UserId = @UserId;
    
    SELECT @@ROWCOUNT;
END
GO

-- Delete User
IF OBJECT_ID('sp_DeleteUser', 'P') IS NOT NULL
    DROP PROCEDURE sp_DeleteUser;
GO

CREATE PROCEDURE sp_DeleteUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM Users WHERE UserId = @UserId;
    
    SELECT @@ROWCOUNT;
END
GO

-- Check Username Exists
IF OBJECT_ID('sp_CheckUsernameExists', 'P') IS NOT NULL
    DROP PROCEDURE sp_CheckUsernameExists;
GO

CREATE PROCEDURE sp_CheckUsernameExists
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT CASE WHEN EXISTS (SELECT 1 FROM Users WHERE Username = @Username) 
                THEN 1 ELSE 0 END;
END
GO

-- Check Email Exists
IF OBJECT_ID('sp_CheckEmailExists', 'P') IS NOT NULL
    DROP PROCEDURE sp_CheckEmailExists;
GO

CREATE PROCEDURE sp_CheckEmailExists
    @Email NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT CASE WHEN EXISTS (SELECT 1 FROM Users WHERE Email = @Email) 
                THEN 1 ELSE 0 END;
END
GO

-- Enhanced Create User (Updated)
IF OBJECT_ID('sp_CreateUser', 'P') IS NOT NULL
    DROP PROCEDURE sp_CreateUser;
GO

CREATE PROCEDURE sp_CreateUser
    @Username NVARCHAR(50),
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(500),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @PhoneNumber NVARCHAR(20) = NULL,
    @IsActive BIT = 1,
    @IsEmailConfirmed BIT = 0,
    @Role NVARCHAR(20) = 'User',
    @ProfileImageUrl NVARCHAR(500) = NULL,
    @Department NVARCHAR(100) = NULL,
    @EmployeeId NVARCHAR(50) = NULL,
    @CreatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, 
                      PhoneNumber, IsActive, IsEmailConfirmed, Role, 
                      ProfileImageUrl, Department, EmployeeId, CreatedBy, 
                      CreatedDate)
    VALUES (@Username, @Email, @PasswordHash, @FirstName, @LastName, 
            @PhoneNumber, @IsActive, @IsEmailConfirmed, @Role, 
            @ProfileImageUrl, @Department, @EmployeeId, @CreatedBy, 
            GETDATE());
    
    SELECT SCOPE_IDENTITY();
END
GO

PRINT 'User Management Stored Procedures created successfully!';