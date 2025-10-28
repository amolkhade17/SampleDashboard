-- Fix missing sp_GetApplicationById stored procedure
USE WebAppMVCDb;
GO

-- Check if procedure exists and create if missing
IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetApplicationById')
BEGIN
    EXEC('
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
    ')
    PRINT 'sp_GetApplicationById created successfully!'
END
ELSE
BEGIN
    PRINT 'sp_GetApplicationById already exists.'
END
GO

-- Test the procedure
PRINT 'Testing sp_GetApplicationById with ApplicationId = 1:'
EXEC sp_GetApplicationById @ApplicationId = 1;
GO