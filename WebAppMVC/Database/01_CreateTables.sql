-- =====================================================
-- Complete Table Schema Creation Script
-- ASP.NET Core 9 MVC Web Application
-- =====================================================

USE WebAppMVCDb;
GO

-- =====================================================
-- 1. USERS TABLE
-- =====================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        UserId INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        PhoneNumber NVARCHAR(20) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsEmailConfirmed BIT NOT NULL DEFAULT 0,
        CreatedDate DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        LastLoginDate DATETIME2(7) NULL,
        LastModifiedDate DATETIME2(7) NULL,
        Role NVARCHAR(20) NOT NULL DEFAULT 'User',
        ProfileImageUrl NVARCHAR(500) NULL,
        Department NVARCHAR(100) NULL,
        EmployeeId NVARCHAR(50) NULL,
        
        -- Constraints
        CONSTRAINT UQ_Users_Username UNIQUE (Username),
        CONSTRAINT UQ_Users_Email UNIQUE (Email),
        CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin', 'Manager', 'User', 'Guest')),
        CONSTRAINT CK_Users_Email_Format CHECK (Email LIKE '%@%.%')
    );
    
    PRINT 'Users table created successfully.';
END
ELSE
BEGIN
    PRINT 'Users table already exists.';
END
GO

-- =====================================================
-- 2. APPLICATIONS TABLE
-- =====================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Applications' AND xtype='U')
BEGIN
    CREATE TABLE Applications (
        ApplicationId INT IDENTITY(1,1) PRIMARY KEY,
        ApplicationName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        IconClass NVARCHAR(50) NOT NULL DEFAULT 'fas fa-cube',
        BackgroundColor NVARCHAR(20) NOT NULL DEFAULT '#2196f3',
        RouteUrl NVARCHAR(200) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        SortOrder INT NOT NULL DEFAULT 0,
        RequiredRole NVARCHAR(20) NULL,
        CreatedDate DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy INT NULL,
        LastModifiedDate DATETIME2(7) NULL,
        LastModifiedBy INT NULL,
        Version NVARCHAR(20) NULL DEFAULT '1.0',
        
        -- Constraints
        CONSTRAINT UQ_Applications_Name UNIQUE (ApplicationName),
        CONSTRAINT CK_Applications_RequiredRole CHECK (RequiredRole IN ('Admin', 'Manager', 'User', 'Guest') OR RequiredRole IS NULL),
        CONSTRAINT FK_Applications_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
        CONSTRAINT FK_Applications_ModifiedBy FOREIGN KEY (LastModifiedBy) REFERENCES Users(UserId)
    );
    
    PRINT 'Applications table created successfully.';
END
ELSE
BEGIN
    PRINT 'Applications table already exists.';
END
GO

-- =====================================================
-- 3. CRUD OPERATIONS TABLE
-- =====================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CrudOperations' AND xtype='U')
BEGIN
    CREATE TABLE CrudOperations (
        OperationId INT IDENTITY(1,1) PRIMARY KEY,
        ApplicationId INT NOT NULL,
        OperationName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        IconClass NVARCHAR(50) NOT NULL DEFAULT 'fas fa-cog',
        BackgroundColor NVARCHAR(20) NOT NULL DEFAULT '#4caf50',
        ActionName NVARCHAR(50) NOT NULL,
        ControllerName NVARCHAR(50) NOT NULL,
        RequiredRole NVARCHAR(20) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        SortOrder INT NOT NULL DEFAULT 0,
        CreatedDate DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy INT NULL,
        LastModifiedDate DATETIME2(7) NULL,
        LastModifiedBy INT NULL,
        
        -- Constraints
        CONSTRAINT FK_CrudOperations_Application FOREIGN KEY (ApplicationId) REFERENCES Applications(ApplicationId) ON DELETE CASCADE,
        CONSTRAINT FK_CrudOperations_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
        CONSTRAINT FK_CrudOperations_ModifiedBy FOREIGN KEY (LastModifiedBy) REFERENCES Users(UserId),
        CONSTRAINT CK_CrudOperations_RequiredRole CHECK (RequiredRole IN ('Admin', 'Manager', 'User', 'Guest') OR RequiredRole IS NULL)
    );
    
    PRINT 'CrudOperations table created successfully.';
END
ELSE
BEGIN
    PRINT 'CrudOperations table already exists.';
END
GO

-- =====================================================
-- 4. PRODUCTS TABLE (Sample Entity for CRUD Operations)
-- =====================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Products' AND xtype='U')
BEGIN
    CREATE TABLE Products (
        ProductId INT IDENTITY(1,1) PRIMARY KEY,
        ProductName NVARCHAR(200) NOT NULL,
        ProductCode NVARCHAR(50) NOT NULL,
        Description NVARCHAR(1000) NULL,
        Category NVARCHAR(100) NOT NULL,
        Price DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        CostPrice DECIMAL(18,2) NULL,
        Quantity INT NOT NULL DEFAULT 0,
        MinimumStock INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        SKU NVARCHAR(100) NULL,
        Barcode NVARCHAR(100) NULL,
        Weight DECIMAL(10,3) NULL,
        Dimensions NVARCHAR(100) NULL,
        Color NVARCHAR(50) NULL,
        Size NVARCHAR(50) NULL,
        Brand NVARCHAR(100) NULL,
        Supplier NVARCHAR(200) NULL,
        CreatedDate DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy INT NULL,
        LastModifiedDate DATETIME2(7) NULL,
        LastModifiedBy INT NULL,
        
        -- Constraints
        CONSTRAINT UQ_Products_Code UNIQUE (ProductCode),
        CONSTRAINT UQ_Products_SKU UNIQUE (SKU),
        CONSTRAINT CK_Products_Price CHECK (Price >= 0),
        CONSTRAINT CK_Products_Quantity CHECK (Quantity >= 0),
        CONSTRAINT FK_Products_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
        CONSTRAINT FK_Products_ModifiedBy FOREIGN KEY (LastModifiedBy) REFERENCES Users(UserId)
    );
    
    PRINT 'Products table created successfully.';
END
ELSE
BEGIN
    PRINT 'Products table already exists.';
END
GO

-- =====================================================
-- 5. AUDIT LOG TABLE (For tracking changes)
-- =====================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AuditLog' AND xtype='U')
BEGIN
    CREATE TABLE AuditLog (
        AuditId BIGINT IDENTITY(1,1) PRIMARY KEY,
        TableName NVARCHAR(100) NOT NULL,
        RecordId NVARCHAR(50) NOT NULL,
        Action NVARCHAR(20) NOT NULL, -- INSERT, UPDATE, DELETE
        OldValues NVARCHAR(MAX) NULL,
        NewValues NVARCHAR(MAX) NULL,
        UserId INT NULL,
        Username NVARCHAR(50) NULL,
        Timestamp DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        IPAddress NVARCHAR(45) NULL,
        UserAgent NVARCHAR(500) NULL,
        
        -- Constraints
        CONSTRAINT CK_AuditLog_Action CHECK (Action IN ('INSERT', 'UPDATE', 'DELETE')),
        CONSTRAINT FK_AuditLog_User FOREIGN KEY (UserId) REFERENCES Users(UserId)
    );
    
    PRINT 'AuditLog table created successfully.';
END
ELSE
BEGIN
    PRINT 'AuditLog table already exists.';
END
GO

-- =====================================================
-- 6. USER SESSIONS TABLE (For JWT token management)
-- =====================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserSessions' AND xtype='U')
BEGIN
    CREATE TABLE UserSessions (
        SessionId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId INT NOT NULL,
        TokenHash NVARCHAR(MAX) NOT NULL,
        ExpiryDate DATETIME2(7) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        IPAddress NVARCHAR(45) NULL,
        UserAgent NVARCHAR(500) NULL,
        LastActivityDate DATETIME2(7) NULL,
        
        -- Constraints
        CONSTRAINT FK_UserSessions_User FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
    );
    
    PRINT 'UserSessions table created successfully.';
END
ELSE
BEGIN
    PRINT 'UserSessions table already exists.';
END
GO

-- =====================================================
-- CREATE INDEXES FOR PERFORMANCE
-- =====================================================

-- Users table indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Username')
    CREATE NONCLUSTERED INDEX IX_Users_Username ON Users (Username);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email')
    CREATE NONCLUSTERED INDEX IX_Users_Email ON Users (Email);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_IsActive')
    CREATE NONCLUSTERED INDEX IX_Users_IsActive ON Users (IsActive);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Role')
    CREATE NONCLUSTERED INDEX IX_Users_Role ON Users (Role);

-- Applications table indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Applications_IsActive_SortOrder')
    CREATE NONCLUSTERED INDEX IX_Applications_IsActive_SortOrder ON Applications (IsActive, SortOrder);

-- CrudOperations table indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CrudOperations_ApplicationId')
    CREATE NONCLUSTERED INDEX IX_CrudOperations_ApplicationId ON CrudOperations (ApplicationId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CrudOperations_IsActive_SortOrder')
    CREATE NONCLUSTERED INDEX IX_CrudOperations_IsActive_SortOrder ON CrudOperations (IsActive, SortOrder);

-- Products table indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_Category')
    CREATE NONCLUSTERED INDEX IX_Products_Category ON Products (Category);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_IsActive')
    CREATE NONCLUSTERED INDEX IX_Products_IsActive ON Products (IsActive);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_ProductCode')
    CREATE NONCLUSTERED INDEX IX_Products_ProductCode ON Products (ProductCode);

-- UserSessions table indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserSessions_UserId')
    CREATE NONCLUSTERED INDEX IX_UserSessions_UserId ON UserSessions (UserId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserSessions_ExpiryDate')
    CREATE NONCLUSTERED INDEX IX_UserSessions_ExpiryDate ON UserSessions (ExpiryDate);

-- AuditLog table indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLog_TableName_RecordId')
    CREATE NONCLUSTERED INDEX IX_AuditLog_TableName_RecordId ON AuditLog (TableName, RecordId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLog_Timestamp')
    CREATE NONCLUSTERED INDEX IX_AuditLog_Timestamp ON AuditLog (Timestamp);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLog_UserId')
    CREATE NONCLUSTERED INDEX IX_AuditLog_UserId ON AuditLog (UserId);

PRINT 'All indexes created successfully.';
GO

PRINT 'Database schema creation completed successfully!';
PRINT 'Tables created: Users, Applications, CrudOperations, Products, AuditLog, UserSessions';
PRINT 'All indexes and constraints applied.';
GO