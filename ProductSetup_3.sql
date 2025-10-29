-- Product Management Setup Script
USE AdminDashboardDB;
GO

-- Create Products table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type in (N'U'))
BEGIN
    CREATE TABLE Products (
        ProductId INT IDENTITY(1,1) PRIMARY KEY,
        ProductCode NVARCHAR(50) NOT NULL UNIQUE,
        ProductName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        Category NVARCHAR(100) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        Stock INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedBy NVARCHAR(100) NOT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedBy NVARCHAR(100) NULL,
        ModifiedDate DATETIME NULL
    );
    PRINT 'Products table created successfully';
END
GO

-- Create index on ProductCode and ProductName for better search performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_ProductCode')
BEGIN
    CREATE INDEX IX_Products_ProductCode ON Products(ProductCode);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_ProductName')
BEGIN
    CREATE INDEX IX_Products_ProductName ON Products(ProductName);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_Category')
BEGIN
    CREATE INDEX IX_Products_Category ON Products(Category);
END
GO

-- Stored Procedure: Get All Products
CREATE OR ALTER PROCEDURE SP_GetAllProducts
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ProductId,
        ProductCode,
        ProductName,
        Description,
        Category,
        Price,
        Stock,
        IsActive,
        CreatedBy,
        CreatedDate,
        ModifiedBy,
        ModifiedDate
    FROM Products
    ORDER BY CreatedDate DESC;
END
GO

-- Stored Procedure: Get Product By ID
CREATE OR ALTER PROCEDURE SP_GetProductById
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ProductId,
        ProductCode,
        ProductName,
        Description,
        Category,
        Price,
        Stock,
        IsActive,
        CreatedBy,
        CreatedDate,
        ModifiedBy,
        ModifiedDate
    FROM Products
    WHERE ProductId = @ProductId;
END
GO

-- Stored Procedure: Create Product
CREATE OR ALTER PROCEDURE SP_CreateProduct
    @ProductCode NVARCHAR(50),
    @ProductName NVARCHAR(200),
    @Description NVARCHAR(500),
    @Category NVARCHAR(100),
    @Price DECIMAL(18,2),
    @Stock INT,
    @IsActive BIT,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if product code already exists
    IF EXISTS (SELECT 1 FROM Products WHERE ProductCode = @ProductCode)
    BEGIN
        RAISERROR('Product code already exists', 16, 1);
        RETURN;
    END
    
    INSERT INTO Products (ProductCode, ProductName, Description, Category, Price, Stock, IsActive, CreatedBy, CreatedDate)
    VALUES (@ProductCode, @ProductName, @Description, @Category, @Price, @Stock, @IsActive, @CreatedBy, GETDATE());
    
    SELECT SCOPE_IDENTITY() AS ProductId;
END
GO

-- Stored Procedure: Update Product
CREATE OR ALTER PROCEDURE SP_UpdateProduct
    @ProductId INT,
    @ProductCode NVARCHAR(50),
    @ProductName NVARCHAR(200),
    @Description NVARCHAR(500),
    @Category NVARCHAR(100),
    @Price DECIMAL(18,2),
    @Stock INT,
    @IsActive BIT,
    @ModifiedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if product code already exists for another product
    IF EXISTS (SELECT 1 FROM Products WHERE ProductCode = @ProductCode AND ProductId != @ProductId)
    BEGIN
        RAISERROR('Product code already exists', 16, 1);
        RETURN;
    END
    
    UPDATE Products
    SET ProductCode = @ProductCode,
        ProductName = @ProductName,
        Description = @Description,
        Category = @Category,
        Price = @Price,
        Stock = @Stock,
        IsActive = @IsActive,
        ModifiedBy = @ModifiedBy,
        ModifiedDate = GETDATE()
    WHERE ProductId = @ProductId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- Stored Procedure: Delete Product
CREATE OR ALTER PROCEDURE SP_DeleteProduct
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM Products WHERE ProductId = @ProductId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- Stored Procedure: Search Products
CREATE OR ALTER PROCEDURE SP_SearchProducts
    @SearchTerm NVARCHAR(200) = NULL,
    @Category NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ProductId,
        ProductCode,
        ProductName,
        Description,
        Category,
        Price,
        Stock,
        IsActive,
        CreatedBy,
        CreatedDate,
        ModifiedBy,
        ModifiedDate
    FROM Products
    WHERE 
        (@SearchTerm IS NULL OR 
         ProductCode LIKE '%' + @SearchTerm + '%' OR 
         ProductName LIKE '%' + @SearchTerm + '%' OR
         Description LIKE '%' + @SearchTerm + '%')
        AND
        (@Category IS NULL OR Category = @Category)
    ORDER BY CreatedDate DESC;
END
GO

-- Insert sample products
IF NOT EXISTS (SELECT 1 FROM Products)
BEGIN
    INSERT INTO Products (ProductCode, ProductName, Description, Category, Price, Stock, IsActive, CreatedBy, CreatedDate)
    VALUES 
        ('PROD001', 'Laptop Dell XPS 15', 'High-performance laptop with 16GB RAM and 512GB SSD', 'Electronics', 1299.99, 25, 1, 'admin', GETDATE()),
        ('PROD002', 'iPhone 15 Pro', 'Latest Apple smartphone with A17 Pro chip', 'Electronics', 999.99, 50, 1, 'admin', GETDATE()),
        ('PROD003', 'Office Chair Premium', 'Ergonomic office chair with lumbar support', 'Furniture', 299.99, 15, 1, 'admin', GETDATE()),
        ('PROD004', 'Wireless Mouse Logitech', 'Bluetooth wireless mouse with precision tracking', 'Accessories', 49.99, 100, 1, 'admin', GETDATE()),
        ('PROD005', 'Standing Desk Electric', 'Height-adjustable standing desk 120x60cm', 'Furniture', 599.99, 10, 1, 'admin', GETDATE()),
        ('PROD006', 'Mechanical Keyboard RGB', 'Gaming mechanical keyboard with RGB backlight', 'Accessories', 129.99, 35, 1, 'admin', GETDATE()),
        ('PROD007', 'Monitor 27" 4K', 'Ultra HD 4K monitor with IPS panel', 'Electronics', 449.99, 20, 1, 'admin', GETDATE()),
        ('PROD008', 'Webcam HD 1080p', 'Full HD webcam with auto-focus and noise reduction', 'Accessories', 79.99, 45, 1, 'admin', GETDATE()),
        ('PROD009', 'Desk Lamp LED', 'Adjustable LED desk lamp with touch control', 'Furniture', 39.99, 60, 1, 'admin', GETDATE()),
        ('PROD010', 'External SSD 1TB', 'Portable external SSD with USB-C connection', 'Electronics', 149.99, 40, 1, 'admin', GETDATE());
    
    PRINT 'Sample products inserted successfully';
END

PRINT 'Product management setup completed successfully!';
GO
