-- Maker-Checker Setup Script
USE AdminDashboardDB;
GO

-- Add Maker and Checker roles
IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Maker')
BEGIN
    INSERT INTO Roles (RoleName, Description)
    VALUES ('Maker', 'Can create and modify records but cannot authorize');
    PRINT 'Maker role created';
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Checker')
BEGIN
    INSERT INTO Roles (RoleName, Description)
    VALUES ('Checker', 'Can authorize records created by Maker');
    PRINT 'Checker role created';
END
GO

-- Create PendingRecords table to store records awaiting approval
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PendingRecords]') AND type in (N'U'))
BEGIN
    CREATE TABLE PendingRecords (
        PendingId INT IDENTITY(1,1) PRIMARY KEY,
        RecordType NVARCHAR(50) NOT NULL, -- 'User', 'Role', etc.
        Operation NVARCHAR(20) NOT NULL, -- 'Create', 'Update', 'Delete'
        RecordId INT NULL, -- Original record ID for Update/Delete operations
        RecordData NVARCHAR(MAX) NOT NULL, -- JSON data of the record
        MakerId INT NOT NULL,
        MakerName NVARCHAR(100) NOT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Approved', 'Rejected'
        CheckerId INT NULL,
        CheckerName NVARCHAR(100) NULL,
        CheckerComments NVARCHAR(500) NULL,
        AuthorizedDate DATETIME NULL,
        FOREIGN KEY (MakerId) REFERENCES Users(UserId)
    );
END
GO

-- Create index on Status for better query performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PendingRecords_Status')
BEGIN
    CREATE INDEX IX_PendingRecords_Status ON PendingRecords(Status);
END
GO

-- Stored Procedure: Create Pending Record
CREATE OR ALTER PROCEDURE SP_CreatePendingRecord
    @RecordType NVARCHAR(50),
    @Operation NVARCHAR(20),
    @RecordId INT = NULL,
    @RecordData NVARCHAR(MAX),
    @MakerId INT,
    @MakerName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO PendingRecords (RecordType, Operation, RecordId, RecordData, MakerId, MakerName, Status, CreatedDate)
    VALUES (@RecordType, @Operation, @RecordId, @RecordData, @MakerId, @MakerName, 'Pending', GETDATE());
    
    SELECT SCOPE_IDENTITY() AS PendingId;
END
GO

-- Stored Procedure: Get All Pending Records
CREATE OR ALTER PROCEDURE SP_GetAllPendingRecords
    @Status NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        PendingId,
        RecordType,
        Operation,
        RecordId,
        RecordData,
        MakerId,
        MakerName,
        CreatedDate,
        Status,
        CheckerId,
        CheckerName,
        CheckerComments,
        AuthorizedDate
    FROM PendingRecords
    WHERE (@Status IS NULL OR Status = @Status)
    ORDER BY CreatedDate DESC;
END
GO

-- Stored Procedure: Get Pending Record By ID
CREATE OR ALTER PROCEDURE SP_GetPendingRecordById
    @PendingId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        PendingId,
        RecordType,
        Operation,
        RecordId,
        RecordData,
        MakerId,
        MakerName,
        CreatedDate,
        Status,
        CheckerId,
        CheckerName,
        CheckerComments,
        AuthorizedDate
    FROM PendingRecords
    WHERE PendingId = @PendingId;
END
GO

-- Stored Procedure: Approve Pending Record
CREATE OR ALTER PROCEDURE SP_ApprovePendingRecord
    @PendingId INT,
    @CheckerId INT,
    @CheckerName NVARCHAR(100),
    @CheckerComments NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    DECLARE @MakerId INT;
    DECLARE @Operation NVARCHAR(20);
    DECLARE @RecordType NVARCHAR(50);
    DECLARE @RecordData NVARCHAR(MAX);
    DECLARE @RecordId INT;
    
    -- Get pending record details
    SELECT 
        @MakerId = MakerId,
        @Operation = Operation,
        @RecordType = RecordType,
        @RecordData = RecordData,
        @RecordId = RecordId
    FROM PendingRecords
    WHERE PendingId = @PendingId AND Status = 'Pending';
    
    -- Check if record exists
    IF @MakerId IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Pending record not found or already processed', 16, 1);
        RETURN;
    END
    
    -- Check if Maker and Checker are the same
    IF @MakerId = @CheckerId
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Checker cannot be the same as Maker', 16, 1);
        RETURN;
    END
    
    -- Update pending record status
    UPDATE PendingRecords
    SET Status = 'Approved',
        CheckerId = @CheckerId,
        CheckerName = @CheckerName,
        CheckerComments = @CheckerComments,
        AuthorizedDate = GETDATE()
    WHERE PendingId = @PendingId;
    
    COMMIT TRANSACTION;
    SELECT 1 AS Success;
END
GO

-- Stored Procedure: Reject Pending Record
CREATE OR ALTER PROCEDURE SP_RejectPendingRecord
    @PendingId INT,
    @CheckerId INT,
    @CheckerName NVARCHAR(100),
    @CheckerComments NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    DECLARE @MakerId INT;
    
    -- Get maker ID
    SELECT @MakerId = MakerId
    FROM PendingRecords
    WHERE PendingId = @PendingId AND Status = 'Pending';
    
    -- Check if record exists
    IF @MakerId IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Pending record not found or already processed', 16, 1);
        RETURN;
    END
    
    -- Check if Maker and Checker are the same
    IF @MakerId = @CheckerId
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Checker cannot be the same as Maker', 16, 1);
        RETURN;
    END
    
    -- Update pending record status
    UPDATE PendingRecords
    SET Status = 'Rejected',
        CheckerId = @CheckerId,
        CheckerName = @CheckerName,
        CheckerComments = @CheckerComments,
        AuthorizedDate = GETDATE()
    WHERE PendingId = @PendingId;
    
    COMMIT TRANSACTION;
    SELECT 1 AS Success;
END
GO

-- Stored Procedure: Execute Approved User Operation
CREATE OR ALTER PROCEDURE SP_ExecuteApprovedUserOperation
    @PendingId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    DECLARE @Operation NVARCHAR(20);
    DECLARE @RecordId INT;
    DECLARE @Username NVARCHAR(100);
    DECLARE @PasswordHash NVARCHAR(255);
    DECLARE @Email NVARCHAR(100);
    DECLARE @FullName NVARCHAR(100);
    DECLARE @RoleId INT;
    DECLARE @IsActive BIT;
    DECLARE @RecordData NVARCHAR(MAX);
    
    -- Get pending record details
    SELECT 
        @Operation = Operation,
        @RecordId = RecordId,
        @RecordData = RecordData
    FROM PendingRecords
    WHERE PendingId = @PendingId AND Status = 'Approved';
    
    IF @Operation IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Pending record not found or not approved', 16, 1);
        RETURN;
    END
    
    -- Parse JSON data
    SELECT 
        @Username = JSON_VALUE(@RecordData, '$.Username'),
        @PasswordHash = JSON_VALUE(@RecordData, '$.PasswordHash'),
        @Email = JSON_VALUE(@RecordData, '$.Email'),
        @FullName = JSON_VALUE(@RecordData, '$.FullName'),
        @RoleId = JSON_VALUE(@RecordData, '$.RoleId'),
        @IsActive = JSON_VALUE(@RecordData, '$.IsActive');
    
    -- Execute operation
    IF @Operation = 'Create'
    BEGIN
        INSERT INTO Users (Username, PasswordHash, Email, FullName, RoleId, IsActive, CreatedDate)
        VALUES (@Username, @PasswordHash, @Email, @FullName, @RoleId, @IsActive, GETDATE());
    END
    ELSE IF @Operation = 'Update'
    BEGIN
        UPDATE Users
        SET Username = @Username,
            Email = @Email,
            FullName = @FullName,
            RoleId = @RoleId,
            IsActive = @IsActive
        WHERE UserId = @RecordId;
    END
    ELSE IF @Operation = 'Delete'
    BEGIN
        DELETE FROM Users WHERE UserId = @RecordId;
    END
    
    COMMIT TRANSACTION;
    SELECT 1 AS Success;
END
GO

-- Create sample Maker and Checker users
DECLARE @MakerRoleId INT, @CheckerRoleId INT;

SELECT @MakerRoleId = RoleId FROM Roles WHERE RoleName = 'Maker';
SELECT @CheckerRoleId = RoleId FROM Roles WHERE RoleName = 'Checker';

-- Check if maker user exists
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'maker')
BEGIN
    -- Password: Maker@123
    INSERT INTO Users (Username, PasswordHash, Email, FullName, RoleId, IsActive, CreatedDate)
    VALUES ('maker', 'oI70zQIJ10/+2FKCj/s+q7ziRcQri5LQuzCRlA3gZn4r0D7xEITOYy3tqRH0a4jD', 
            'maker@company.com', 'Maker User', @MakerRoleId, 1, GETDATE());
    PRINT 'Maker user created - Username: maker, Password: Maker@123';
END

-- Check if checker user exists
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'checker')
BEGIN
    -- Password: Checker@123
    INSERT INTO Users (Username, PasswordHash, Email, FullName, RoleId, IsActive, CreatedDate)
    VALUES ('checker', 'NJpFly+l51Zvp7ZglisBe6YHCrCIYu89gIhi4pK8/fhQ1nQ+tjE0NMvYldeBLg9M', 
            'checker@company.com', 'Checker User', @CheckerRoleId, 1, GETDATE());
    PRINT 'Checker user created - Username: checker, Password: Checker@123';
END

PRINT 'Maker-Checker setup completed successfully!';
GO
