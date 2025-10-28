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
        EmployeeId
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
        EmployeeId
    FROM Users 
    WHERE Email = @Email AND IsActive = 1;
END
GO

-- Procedure: Authenticate User
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_AuthenticateUser')
    DROP PROCEDURE sp_AuthenticateUser;
GO

CREATE PROCEDURE sp_AuthenticateUser
    @Username NVARCHAR(50),
    @PasswordHash NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserId INT;
    
    SELECT 
        @UserId = UserId,
        UserId,
        Username,
        Email,
        FirstName,
        LastName,
        Role,
        IsActive,
        IsEmailConfirmed
    FROM Users 
    WHERE (Username = @Username OR Email = @Username) 
          AND PasswordHash = @PasswordHash 
          AND IsActive = 1;
    
    -- Update last login date if user found
    IF @UserId IS NOT NULL
    BEGIN
        UPDATE Users 
        SET LastLoginDate = GETUTCDATE()
        WHERE UserId = @UserId;
    END
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
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(100) = NULL,
    @Role NVARCHAR(20) = NULL,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Get total count
    DECLARE @TotalCount INT;
    
    SELECT @TotalCount = COUNT(*)
    FROM Users u
    WHERE (@SearchTerm IS NULL OR 
           u.Username LIKE '%' + @SearchTerm + '%' OR 
           u.Email LIKE '%' + @SearchTerm + '%' OR 
           u.FirstName LIKE '%' + @SearchTerm + '%' OR 
           u.LastName LIKE '%' + @SearchTerm + '%')
      AND (@Role IS NULL OR u.Role = @Role)
      AND (@IsActive IS NULL OR u.IsActive = @IsActive);
    
    -- Get paged results
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
        Role,
        Department,
        EmployeeId,
        @TotalCount as TotalCount
    FROM Users u
    WHERE (@SearchTerm IS NULL OR 
           u.Username LIKE '%' + @SearchTerm + '%' OR 
           u.Email LIKE '%' + @SearchTerm + '%' OR 
           u.FirstName LIKE '%' + @SearchTerm + '%' OR 
           u.LastName LIKE '%' + @SearchTerm + '%')
      AND (@Role IS NULL OR u.Role = @Role)
      AND (@IsActive IS NULL OR u.IsActive = @IsActive)
    ORDER BY u.CreatedDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
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
        EmployeeId
    FROM Users 
    WHERE UserId = @UserId;
END
GO

-- Procedure: Create User
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
        BEGIN TRANSACTION;
        
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
        
        -- Insert new user
        INSERT INTO Users (
            Username, Email, PasswordHash, FirstName, LastName, 
            PhoneNumber, Role, Department, EmployeeId, 
            IsActive, IsEmailConfirmed, CreatedDate
        )
        VALUES (
            @Username, @Email, @PasswordHash, @FirstName, @LastName,
            @PhoneNumber, @Role, @Department, @EmployeeId,
            1, 0, GETUTCDATE()
        );
        
        SET @NewUserId = SCOPE_IDENTITY();
        
        -- Log audit trail
        INSERT INTO AuditLog (TableName, RecordId, Action, NewValues, UserId, Username, Timestamp)
        VALUES ('Users', CAST(@NewUserId AS NVARCHAR(50)), 'INSERT', 
                'Username: ' + @Username + ', Email: ' + @Email + ', Role: ' + @Role,
                @CreatedBy, 
                (SELECT Username FROM Users WHERE UserId = @CreatedBy),
                GETUTCDATE());
        
        COMMIT TRANSACTION;
        
        SELECT @NewUserId AS UserId;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
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
    
    DECLARE @OldValues NVARCHAR(MAX);
    DECLARE @NewValues NVARCHAR(MAX);
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
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
        
        -- Get old values for audit
        SELECT @OldValues = CONCAT(
            'Username: ', Username, ', Email: ', Email, ', FirstName: ', FirstName,
            ', LastName: ', LastName, ', Role: ', Role, ', IsActive: ', CAST(IsActive AS NVARCHAR(10))
        )
        FROM Users WHERE UserId = @UserId;
        
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
            LastModifiedDate = GETUTCDATE()
        WHERE UserId = @UserId;
        
        -- Set new values for audit
        SET @NewValues = CONCAT(
            'Username: ', @Username, ', Email: ', @Email, ', FirstName: ', @FirstName,
            ', LastName: ', @LastName, ', Role: ', @Role, ', IsActive: ', CAST(@IsActive AS NVARCHAR(10))
        );
        
        -- Log audit trail
        INSERT INTO AuditLog (TableName, RecordId, Action, OldValues, NewValues, UserId, Username, Timestamp)
        VALUES ('Users', CAST(@UserId AS NVARCHAR(50)), 'UPDATE', @OldValues, @NewValues,
                @ModifiedBy, 
                (SELECT Username FROM Users WHERE UserId = @ModifiedBy),
                GETUTCDATE());
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS Success;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Procedure: Delete User (Soft Delete)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_DeleteUser')
    DROP PROCEDURE sp_DeleteUser;
GO

CREATE PROCEDURE sp_DeleteUser
    @UserId INT,
    @DeletedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @OldValues NVARCHAR(MAX);
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if user exists and is active
        IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId AND IsActive = 1)
        BEGIN
            RAISERROR('Active user not found', 16, 1);
            RETURN -1;
        END
        
        -- Get old values for audit
        SELECT @OldValues = CONCAT(
            'Username: ', Username, ', Email: ', Email, ', FirstName: ', FirstName,
            ', LastName: ', LastName, ', Role: ', Role
        )
        FROM Users WHERE UserId = @UserId;
        
        -- Soft delete user
        UPDATE Users 
        SET IsActive = 0,
            LastModifiedDate = GETUTCDATE()
        WHERE UserId = @UserId;
        
        -- Deactivate user sessions
        UPDATE UserSessions 
        SET IsActive = 0 
        WHERE UserId = @UserId;
        
        -- Log audit trail
        INSERT INTO AuditLog (TableName, RecordId, Action, OldValues, UserId, Username, Timestamp)
        VALUES ('Users', CAST(@UserId AS NVARCHAR(50)), 'DELETE', @OldValues,
                @DeletedBy, 
                (SELECT Username FROM Users WHERE UserId = @DeletedBy),
                GETUTCDATE());
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS Success;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =====================================================
-- 3. SESSION MANAGEMENT PROCEDURES
-- =====================================================

-- Procedure: Create User Session
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CreateUserSession')
    DROP PROCEDURE sp_CreateUserSession;
GO

CREATE PROCEDURE sp_CreateUserSession
    @UserId INT,
    @TokenHash NVARCHAR(MAX),
    @ExpiryDate DATETIME2(7),
    @IPAddress NVARCHAR(45) = NULL,
    @UserAgent NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SessionId UNIQUEIDENTIFIER = NEWID();
    
    BEGIN TRY
        -- Clean up expired sessions
        DELETE FROM UserSessions 
        WHERE UserId = @UserId AND ExpiryDate < GETUTCDATE();
        
        -- Insert new session
        INSERT INTO UserSessions (SessionId, UserId, TokenHash, ExpiryDate, IPAddress, UserAgent, IsActive, CreatedDate, LastActivityDate)
        VALUES (@SessionId, @UserId, @TokenHash, @ExpiryDate, @IPAddress, @UserAgent, 1, GETUTCDATE(), GETUTCDATE());
        
        SELECT @SessionId AS SessionId;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- Procedure: Validate User Session
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ValidateUserSession')
    DROP PROCEDURE sp_ValidateUserSession;
GO

CREATE PROCEDURE sp_ValidateUserSession
    @SessionId UNIQUEIDENTIFIER,
    @TokenHash NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        s.SessionId,
        s.UserId,
        u.Username,
        u.Role,
        s.ExpiryDate
    FROM UserSessions s
    INNER JOIN Users u ON s.UserId = u.UserId
    WHERE s.SessionId = @SessionId 
          AND s.TokenHash = @TokenHash 
          AND s.IsActive = 1 
          AND s.ExpiryDate > GETUTCDATE()
          AND u.IsActive = 1;
    
    -- Update last activity
    IF @@ROWCOUNT > 0
    BEGIN
        UPDATE UserSessions 
        SET LastActivityDate = GETUTCDATE()
        WHERE SessionId = @SessionId;
    END
END
GO

-- Procedure: Revoke User Session
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_RevokeUserSession')
    DROP PROCEDURE sp_RevokeUserSession;
GO

CREATE PROCEDURE sp_RevokeUserSession
    @SessionId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE UserSessions 
    SET IsActive = 0 
    WHERE SessionId = @SessionId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- Procedure: Cleanup Expired Sessions
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CleanupExpiredSessions')
    DROP PROCEDURE sp_CleanupExpiredSessions;
GO

CREATE PROCEDURE sp_CleanupExpiredSessions
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM UserSessions 
    WHERE ExpiryDate < GETUTCDATE() OR IsActive = 0;
    
    SELECT @@ROWCOUNT AS DeletedSessions;
END
GO

PRINT 'User management stored procedures created successfully!';
PRINT 'Procedures: Authentication, CRUD operations, Session management';
GO