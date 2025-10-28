-- Stored Procedures for Application and CRUD Operations Management
USE WebAppMVCDb;
GO

-- Stored Procedure: Get Active Applications
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetActiveApplications')
    DROP PROCEDURE sp_GetActiveApplications;
GO

CREATE PROCEDURE sp_GetActiveApplications
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
        CreatedDate
    FROM Applications 
    WHERE IsActive = 1
    ORDER BY SortOrder, ApplicationName;
END
GO

-- Stored Procedure: Get Application by ID
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
        CreatedDate
    FROM Applications 
    WHERE ApplicationId = @ApplicationId;
END
GO

-- Stored Procedure: Get Application Operations
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetApplicationOperations')
    DROP PROCEDURE sp_GetApplicationOperations;
GO

CREATE PROCEDURE sp_GetApplicationOperations
    @ApplicationId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        OperationId,
        ApplicationId,
        OperationName,
        Description,
        IconClass,
        BackgroundColor,
        ActionName,
        ControllerName,
        IsActive,
        SortOrder,
        CreatedDate
    FROM CrudOperations 
    WHERE ApplicationId = @ApplicationId 
    AND IsActive = 1
    ORDER BY SortOrder, OperationName;
END
GO

-- Stored Procedure: Create Application
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CreateApplication')
    DROP PROCEDURE sp_CreateApplication;
GO

CREATE PROCEDURE sp_CreateApplication
    @ApplicationName NVARCHAR(100),
    @Description NVARCHAR(500),
    @IconClass NVARCHAR(50),
    @BackgroundColor NVARCHAR(20),
    @RouteUrl NVARCHAR(200),
    @SortOrder INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO Applications (ApplicationName, Description, IconClass, BackgroundColor, RouteUrl, SortOrder, IsActive, CreatedDate)
        VALUES (@ApplicationName, @Description, @IconClass, @BackgroundColor, @RouteUrl, @SortOrder, 1, GETUTCDATE());
        
        SELECT SCOPE_IDENTITY() AS ApplicationId;
        RETURN 1; -- Success
    END TRY
    BEGIN CATCH
        RETURN 0; -- Failure
    END CATCH
END
GO

-- Stored Procedure: Update Application
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_UpdateApplication')
    DROP PROCEDURE sp_UpdateApplication;
GO

CREATE PROCEDURE sp_UpdateApplication
    @ApplicationId INT,
    @ApplicationName NVARCHAR(100),
    @Description NVARCHAR(500),
    @IconClass NVARCHAR(50),
    @BackgroundColor NVARCHAR(20),
    @RouteUrl NVARCHAR(200),
    @SortOrder INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Applications 
    SET 
        ApplicationName = @ApplicationName,
        Description = @Description,
        IconClass = @IconClass,
        BackgroundColor = @BackgroundColor,
        RouteUrl = @RouteUrl,
        SortOrder = @SortOrder,
        IsActive = @IsActive
    WHERE ApplicationId = @ApplicationId;
    
    RETURN @@ROWCOUNT;
END
GO

-- Stored Procedure: Delete Application
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_DeleteApplication')
    DROP PROCEDURE sp_DeleteApplication;
GO

CREATE PROCEDURE sp_DeleteApplication
    @ApplicationId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Soft delete by setting IsActive to 0
        UPDATE Applications 
        SET IsActive = 0
        WHERE ApplicationId = @ApplicationId;
        
        -- Also deactivate related operations
        UPDATE CrudOperations 
        SET IsActive = 0
        WHERE ApplicationId = @ApplicationId;
        
        RETURN @@ROWCOUNT;
    END TRY
    BEGIN CATCH
        RETURN 0;
    END CATCH
END
GO

-- Stored Procedure: Create CRUD Operation
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CreateCrudOperation')
    DROP PROCEDURE sp_CreateCrudOperation;
GO

CREATE PROCEDURE sp_CreateCrudOperation
    @ApplicationId INT,
    @OperationName NVARCHAR(100),
    @Description NVARCHAR(500),
    @IconClass NVARCHAR(50),
    @BackgroundColor NVARCHAR(20),
    @ActionName NVARCHAR(50),
    @ControllerName NVARCHAR(50),
    @SortOrder INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO CrudOperations (ApplicationId, OperationName, Description, IconClass, BackgroundColor, ActionName, ControllerName, SortOrder, IsActive, CreatedDate)
        VALUES (@ApplicationId, @OperationName, @Description, @IconClass, @BackgroundColor, @ActionName, @ControllerName, @SortOrder, 1, GETUTCDATE());
        
        SELECT SCOPE_IDENTITY() AS OperationId;
        RETURN 1; -- Success
    END TRY
    BEGIN CATCH
        RETURN 0; -- Failure
    END CATCH
END
GO

-- Stored Procedure: Update CRUD Operation
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_UpdateCrudOperation')
    DROP PROCEDURE sp_UpdateCrudOperation;
GO

CREATE PROCEDURE sp_UpdateCrudOperation
    @OperationId INT,
    @OperationName NVARCHAR(100),
    @Description NVARCHAR(500),
    @IconClass NVARCHAR(50),
    @BackgroundColor NVARCHAR(20),
    @ActionName NVARCHAR(50),
    @ControllerName NVARCHAR(50),
    @SortOrder INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE CrudOperations 
    SET 
        OperationName = @OperationName,
        Description = @Description,
        IconClass = @IconClass,
        BackgroundColor = @BackgroundColor,
        ActionName = @ActionName,
        ControllerName = @ControllerName,
        SortOrder = @SortOrder,
        IsActive = @IsActive
    WHERE OperationId = @OperationId;
    
    RETURN @@ROWCOUNT;
END
GO

-- Stored Procedure: Delete CRUD Operation
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_DeleteCrudOperation')
    DROP PROCEDURE sp_DeleteCrudOperation;
GO

CREATE PROCEDURE sp_DeleteCrudOperation
    @OperationId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Soft delete by setting IsActive to 0
    UPDATE CrudOperations 
    SET IsActive = 0
    WHERE OperationId = @OperationId;
    
    RETURN @@ROWCOUNT;
END
GO

PRINT 'Application and CRUD operation stored procedures created successfully!';