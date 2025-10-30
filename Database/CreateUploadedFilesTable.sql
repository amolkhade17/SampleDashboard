-- Create UploadedFiles Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UploadedFiles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UploadedFiles](
        [FileId] [int] IDENTITY(1,1) NOT NULL,
        [FileName] [nvarchar](255) NOT NULL,
        [OriginalFileName] [nvarchar](255) NOT NULL,
        [FilePath] [nvarchar](500) NOT NULL,
        [FileSize] [bigint] NOT NULL,
        [FileExtension] [nvarchar](50) NOT NULL,
        [MimeType] [nvarchar](100) NULL,
        [UploadedBy] [nvarchar](100) NOT NULL,
        [UploadedDate] [datetime] NOT NULL DEFAULT GETDATE(),
        [ModifiedBy] [nvarchar](100) NULL,
        [ModifiedDate] [datetime] NULL,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [DeletedBy] [nvarchar](100) NULL,
        [DeletedDate] [datetime] NULL,
        CONSTRAINT [PK_UploadedFiles] PRIMARY KEY CLUSTERED ([FileId] ASC)
    )
END
GO

-- Create Index on FileName for faster searches
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UploadedFiles_FileName' AND object_id = OBJECT_ID('UploadedFiles'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_UploadedFiles_FileName] ON [dbo].[UploadedFiles]
    (
        [FileName] ASC
    )
END
GO

-- Create Index on UploadedBy
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UploadedFiles_UploadedBy' AND object_id = OBJECT_ID('UploadedFiles'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_UploadedFiles_UploadedBy] ON [dbo].[UploadedFiles]
    (
        [UploadedBy] ASC
    )
END
GO

-- =============================================
-- Stored Procedure: SP_CreateUploadedFile
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_CreateUploadedFile]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_CreateUploadedFile]
GO

CREATE PROCEDURE [dbo].[SP_CreateUploadedFile]
    @FileName NVARCHAR(255),
    @OriginalFileName NVARCHAR(255),
    @FilePath NVARCHAR(500),
    @FileSize BIGINT,
    @FileExtension NVARCHAR(50),
    @MimeType NVARCHAR(100),
    @UploadedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO UploadedFiles (FileName, OriginalFileName, FilePath, FileSize, FileExtension, MimeType, UploadedBy, UploadedDate)
    VALUES (@FileName, @OriginalFileName, @FilePath, @FileSize, @FileExtension, @MimeType, @UploadedBy, GETDATE())
    
    SELECT SCOPE_IDENTITY() AS FileId
END
GO

-- =============================================
-- Stored Procedure: SP_GetAllUploadedFiles
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_GetAllUploadedFiles]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_GetAllUploadedFiles]
GO

CREATE PROCEDURE [dbo].[SP_GetAllUploadedFiles]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        FileId,
        FileName,
        OriginalFileName,
        FilePath,
        FileSize,
        FileExtension,
        MimeType,
        UploadedBy,
        UploadedDate,
        ModifiedBy,
        ModifiedDate,
        IsDeleted
    FROM UploadedFiles
    WHERE IsDeleted = 0
    ORDER BY UploadedDate DESC
END
GO

-- =============================================
-- Stored Procedure: SP_GetUploadedFileById
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_GetUploadedFileById]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_GetUploadedFileById]
GO

CREATE PROCEDURE [dbo].[SP_GetUploadedFileById]
    @FileId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        FileId,
        FileName,
        OriginalFileName,
        FilePath,
        FileSize,
        FileExtension,
        MimeType,
        UploadedBy,
        UploadedDate,
        ModifiedBy,
        ModifiedDate,
        IsDeleted
    FROM UploadedFiles
    WHERE FileId = @FileId AND IsDeleted = 0
END
GO

-- =============================================
-- Stored Procedure: SP_GetUploadedFileByFileName
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_GetUploadedFileByFileName]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_GetUploadedFileByFileName]
GO

CREATE PROCEDURE [dbo].[SP_GetUploadedFileByFileName]
    @FileName NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        FileId,
        FileName,
        OriginalFileName,
        FilePath,
        FileSize,
        FileExtension,
        MimeType,
        UploadedBy,
        UploadedDate,
        ModifiedBy,
        ModifiedDate,
        IsDeleted
    FROM UploadedFiles
    WHERE FileName = @FileName AND IsDeleted = 0
END
GO

-- =============================================
-- Stored Procedure: SP_UpdateUploadedFile
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_UpdateUploadedFile]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_UpdateUploadedFile]
GO

CREATE PROCEDURE [dbo].[SP_UpdateUploadedFile]
    @FileId INT,
    @FileName NVARCHAR(255),
    @ModifiedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE UploadedFiles
    SET 
        FileName = @FileName,
        ModifiedBy = @ModifiedBy,
        ModifiedDate = GETDATE()
    WHERE FileId = @FileId AND IsDeleted = 0
END
GO

-- =============================================
-- Stored Procedure: SP_DeleteUploadedFile (Soft Delete)
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_DeleteUploadedFile]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_DeleteUploadedFile]
GO

CREATE PROCEDURE [dbo].[SP_DeleteUploadedFile]
    @FileName NVARCHAR(255),
    @DeletedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE UploadedFiles
    SET 
        IsDeleted = 1,
        DeletedBy = @DeletedBy,
        DeletedDate = GETDATE()
    WHERE FileName = @FileName AND IsDeleted = 0
END
GO

-- =============================================
-- Stored Procedure: SP_GetFileStatistics
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_GetFileStatistics]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_GetFileStatistics]
GO

CREATE PROCEDURE [dbo].[SP_GetFileStatistics]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        COUNT(*) AS TotalFiles,
        SUM(FileSize) AS TotalSize,
        MAX(UploadedDate) AS LastUploadDate,
        COUNT(DISTINCT UploadedBy) AS TotalUploaders
    FROM UploadedFiles
    WHERE IsDeleted = 0
END
GO

PRINT 'UploadedFiles table and stored procedures created successfully!'
