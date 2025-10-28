-- =====================================================
-- ASP.NET Core 9 MVC Web Application Database Setup
-- Complete Database Creation and Configuration Script
-- =====================================================

-- Step 1: Create Database
USE master;
GO

-- Drop database if exists (USE WITH CAUTION IN PRODUCTION!)
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'WebAppMVCDb')
BEGIN
    ALTER DATABASE WebAppMVCDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE WebAppMVCDb;
    PRINT 'Existing database dropped.';
END
GO

-- Create new database
CREATE DATABASE WebAppMVCDb
COLLATE SQL_Latin1_General_CP1_CI_AS;
GO

ALTER DATABASE WebAppMVCDb SET RECOVERY SIMPLE;
GO

USE WebAppMVCDb;
GO

PRINT 'Database WebAppMVCDb created successfully.';
GO

-- Step 2: Create File Groups for better performance (Optional)
-- ALTER DATABASE WebAppMVCDb ADD FILEGROUP DataFG;
-- ALTER DATABASE WebAppMVCDb ADD FILEGROUP IndexFG;

-- Step 3: Create Database Roles and Users (if needed)
-- Create application user for connection pooling
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'WebAppUser')
BEGIN
    CREATE USER WebAppUser WITHOUT LOGIN;
    ALTER ROLE db_datareader ADD MEMBER WebAppUser;
    ALTER ROLE db_datawriter ADD MEMBER WebAppUser;
    ALTER ROLE db_executor ADD MEMBER WebAppUser;
    PRINT 'Database user WebAppUser created with appropriate permissions.';
END
GO