-- Database Creation Script for WebAppMVC
-- Run this script to create the database structure

USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'WebAppMVCDb')
BEGIN
    CREATE DATABASE WebAppMVCDb;
END
GO

USE WebAppMVCDb;
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        UserId INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastLoginDate DATETIME2 NULL,
        Role NVARCHAR(20) NOT NULL DEFAULT 'User'
    );
END
GO

-- Create Applications table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Applications' AND xtype='U')
BEGIN
    CREATE TABLE Applications (
        ApplicationId INT IDENTITY(1,1) PRIMARY KEY,
        ApplicationName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        IconClass NVARCHAR(50) NOT NULL,
        BackgroundColor NVARCHAR(20) NOT NULL,
        RouteUrl NVARCHAR(200) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        SortOrder INT NOT NULL DEFAULT 0,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- Create CrudOperations table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CrudOperations' AND xtype='U')
BEGIN
    CREATE TABLE CrudOperations (
        OperationId INT IDENTITY(1,1) PRIMARY KEY,
        ApplicationId INT NOT NULL,
        OperationName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        IconClass NVARCHAR(50) NOT NULL,
        BackgroundColor NVARCHAR(20) NOT NULL,
        ActionName NVARCHAR(50) NOT NULL,
        ControllerName NVARCHAR(50) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        SortOrder INT NOT NULL DEFAULT 0,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY (ApplicationId) REFERENCES Applications(ApplicationId)
    );
END
GO

-- Insert sample data
-- Create admin user (password: admin123)
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, Role, IsActive)
    VALUES ('admin', 'admin@webappmvc.com', 'mI2L8K4oHCN8jPr1WNQP+HMrOq1pHGWCtcG8Y5FGPlQTXLvJ8P7qKhQSPK1k8GBODKgF5K1KFgvR8K1KFgvR8K1KFgvR8K1KFgvR8==', 'Admin', 'User', 'Admin', 1);
END
GO

-- Create sample user (password: user123)
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'user')
BEGIN
    INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, Role, IsActive)
    VALUES ('user', 'user@webappmvc.com', 'nJ3M9L5pICO9kQs2XORQ+INsPs2qIHXDudH9Z6GHQmRUYMwK9Q8rLiRTQL2l9HCPELhG6L2LGhwS9L2LGhwS9L2LGhwS9L2LGhwS9==', 'Regular', 'User', 'User', 1);
END
GO

-- Insert sample applications
IF NOT EXISTS (SELECT * FROM Applications WHERE ApplicationName = 'User Management')
BEGIN
    INSERT INTO Applications (ApplicationName, Description, IconClass, BackgroundColor, RouteUrl, SortOrder)
    VALUES 
    ('User Management', 'Manage system users and their permissions', 'fas fa-users', '#3f51b5', '/UserManagement', 1),
    ('Product Management', 'Manage products, categories, and inventory', 'fas fa-box', '#4caf50', '/ProductManagement', 2),
    ('Order Management', 'Process and track customer orders', 'fas fa-shopping-cart', '#ff9800', '/OrderManagement', 3),
    ('Report Center', 'Generate and view system reports', 'fas fa-chart-bar', '#9c27b0', '/Reports', 4),
    ('Settings', 'Configure system settings and preferences', 'fas fa-cog', '#607d8b', '/Settings', 5);
END
GO

-- Insert sample CRUD operations for User Management
IF NOT EXISTS (SELECT * FROM CrudOperations WHERE OperationName = 'View Users')
BEGIN
    INSERT INTO CrudOperations (ApplicationId, OperationName, Description, IconClass, BackgroundColor, ActionName, ControllerName, SortOrder)
    VALUES 
    (1, 'View Users', 'View list of all users in the system', 'fas fa-eye', '#2196f3', 'Index', 'Users', 1),
    (1, 'Add User', 'Add new user to the system', 'fas fa-plus', '#4caf50', 'Create', 'Users', 2),
    (1, 'Edit User', 'Modify existing user information', 'fas fa-edit', '#ff9800', 'Edit', 'Users', 3),
    (1, 'Delete User', 'Remove user from the system', 'fas fa-trash', '#f44336', 'Delete', 'Users', 4);
END
GO

-- Insert sample CRUD operations for Product Management
IF NOT EXISTS (SELECT * FROM CrudOperations WHERE ApplicationId = 2 AND OperationName = 'View Products')
BEGIN
    INSERT INTO CrudOperations (ApplicationId, OperationName, Description, IconClass, BackgroundColor, ActionName, ControllerName, SortOrder)
    VALUES 
    (2, 'View Products', 'View list of all products', 'fas fa-eye', '#2196f3', 'Index', 'Products', 1),
    (2, 'Add Product', 'Add new product to inventory', 'fas fa-plus', '#4caf50', 'Create', 'Products', 2),
    (2, 'Edit Product', 'Modify existing product information', 'fas fa-edit', '#ff9800', 'Edit', 'Products', 3),
    (2, 'Delete Product', 'Remove product from inventory', 'fas fa-trash', '#f44336', 'Delete', 'Products', 4);
END
GO

PRINT 'Database schema and sample data created successfully!';