-- Create the missing sp_GetApplicationById stored procedure
-- This procedure was missing from the original script execution

USE WebAppMVCDb;
GO

-- Drop procedure if it exists
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetApplicationById')
    DROP PROCEDURE sp_GetApplicationById;
GO

-- Create the stored procedure
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

-- Test the procedure with existing applications
PRINT 'Testing sp_GetApplicationById:';
EXEC sp_GetApplicationById @ApplicationId = 1;

PRINT 'sp_GetApplicationById created and tested successfully!';
GO