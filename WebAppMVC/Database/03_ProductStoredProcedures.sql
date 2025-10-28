-- =====================================================
-- Product Management Stored Procedures
-- ASP.NET Core 9 MVC Web Application
-- =====================================================

USE WebAppMVCDb;
GO

-- =====================================================
-- 1. PRODUCT CRUD PROCEDURES
-- =====================================================

-- Procedure: Get All Products
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetAllProducts')
    DROP PROCEDURE sp_GetAllProducts;
GO

CREATE PROCEDURE sp_GetAllProducts
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(100) = NULL,
    @Category NVARCHAR(100) = NULL,
    @IsActive BIT = NULL,
    @MinPrice DECIMAL(18,2) = NULL,
    @MaxPrice DECIMAL(18,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Get total count
    DECLARE @TotalCount INT;
    
    SELECT @TotalCount = COUNT(*)
    FROM Products p
    WHERE (@SearchTerm IS NULL OR 
           p.ProductName LIKE '%' + @SearchTerm + '%' OR 
           p.ProductCode LIKE '%' + @SearchTerm + '%' OR 
           p.Description LIKE '%' + @SearchTerm + '%' OR
           p.Brand LIKE '%' + @SearchTerm + '%' OR
           p.SKU LIKE '%' + @SearchTerm + '%')
      AND (@Category IS NULL OR p.Category = @Category)
      AND (@IsActive IS NULL OR p.IsActive = @IsActive)
      AND (@MinPrice IS NULL OR p.Price >= @MinPrice)
      AND (@MaxPrice IS NULL OR p.Price <= @MaxPrice);
    
    -- Get paged results
    SELECT 
        ProductId,
        ProductName,
        ProductCode,
        Description,
        Category,
        Price,
        CostPrice,
        Quantity,
        MinimumStock,
        IsActive,
        SKU,
        Barcode,
        Weight,
        Dimensions,
        Color,
        Size,
        Brand,
        Supplier,
        CreatedDate,
        LastModifiedDate,
        @TotalCount as TotalCount,
        -- Calculated fields
        CASE 
            WHEN Quantity <= MinimumStock THEN 'Low Stock'
            WHEN Quantity = 0 THEN 'Out of Stock'
            ELSE 'In Stock'
        END AS StockStatus,
        (Price - ISNULL(CostPrice, 0)) AS ProfitMargin
    FROM Products p
    WHERE (@SearchTerm IS NULL OR 
           p.ProductName LIKE '%' + @SearchTerm + '%' OR 
           p.ProductCode LIKE '%' + @SearchTerm + '%' OR 
           p.Description LIKE '%' + @SearchTerm + '%' OR
           p.Brand LIKE '%' + @SearchTerm + '%' OR
           p.SKU LIKE '%' + @SearchTerm + '%')
      AND (@Category IS NULL OR p.Category = @Category)
      AND (@IsActive IS NULL OR p.IsActive = @IsActive)
      AND (@MinPrice IS NULL OR p.Price >= @MinPrice)
      AND (@MaxPrice IS NULL OR p.Price <= @MaxPrice)
    ORDER BY p.CreatedDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Procedure: Get Product by ID
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetProductById')
    DROP PROCEDURE sp_GetProductById;
GO

CREATE PROCEDURE sp_GetProductById
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ProductId,
        ProductName,
        ProductCode,
        Description,
        Category,
        Price,
        CostPrice,
        Quantity,
        MinimumStock,
        IsActive,
        SKU,
        Barcode,
        Weight,
        Dimensions,
        Color,
        Size,
        Brand,
        Supplier,
        CreatedDate,
        CreatedBy,
        LastModifiedDate,
        LastModifiedBy,
        -- Calculated fields
        CASE 
            WHEN Quantity <= MinimumStock THEN 'Low Stock'
            WHEN Quantity = 0 THEN 'Out of Stock'
            ELSE 'In Stock'
        END AS StockStatus,
        (Price - ISNULL(CostPrice, 0)) AS ProfitMargin
    FROM Products 
    WHERE ProductId = @ProductId;
END
GO

-- Procedure: Create Product
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CreateProduct')
    DROP PROCEDURE sp_CreateProduct;
GO

CREATE PROCEDURE sp_CreateProduct
    @ProductName NVARCHAR(200),
    @ProductCode NVARCHAR(50),
    @Description NVARCHAR(1000) = NULL,
    @Category NVARCHAR(100),
    @Price DECIMAL(18,2),
    @CostPrice DECIMAL(18,2) = NULL,
    @Quantity INT = 0,
    @MinimumStock INT = 0,
    @SKU NVARCHAR(100) = NULL,
    @Barcode NVARCHAR(100) = NULL,
    @Weight DECIMAL(10,3) = NULL,
    @Dimensions NVARCHAR(100) = NULL,
    @Color NVARCHAR(50) = NULL,
    @Size NVARCHAR(50) = NULL,
    @Brand NVARCHAR(100) = NULL,
    @Supplier NVARCHAR(200) = NULL,
    @CreatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @NewProductId INT;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if product code already exists
        IF EXISTS (SELECT 1 FROM Products WHERE ProductCode = @ProductCode)
        BEGIN
            RAISERROR('Product code already exists', 16, 1);
            RETURN -1;
        END
        
        -- Check if SKU already exists (if provided)
        IF @SKU IS NOT NULL AND EXISTS (SELECT 1 FROM Products WHERE SKU = @SKU)
        BEGIN
            RAISERROR('SKU already exists', 16, 1);
            RETURN -2;
        END
        
        -- Insert new product
        INSERT INTO Products (
            ProductName, ProductCode, Description, Category, Price, CostPrice,
            Quantity, MinimumStock, SKU, Barcode, Weight, Dimensions,
            Color, Size, Brand, Supplier, IsActive, CreatedDate, CreatedBy
        )
        VALUES (
            @ProductName, @ProductCode, @Description, @Category, @Price, @CostPrice,
            @Quantity, @MinimumStock, @SKU, @Barcode, @Weight, @Dimensions,
            @Color, @Size, @Brand, @Supplier, 1, GETUTCDATE(), @CreatedBy
        );
        
        SET @NewProductId = SCOPE_IDENTITY();
        
        -- Log audit trail
        INSERT INTO AuditLog (TableName, RecordId, Action, NewValues, UserId, Username, Timestamp)
        VALUES ('Products', CAST(@NewProductId AS NVARCHAR(50)), 'INSERT', 
                'ProductName: ' + @ProductName + ', ProductCode: ' + @ProductCode + ', Category: ' + @Category + ', Price: ' + CAST(@Price AS NVARCHAR(20)),
                @CreatedBy, 
                (SELECT Username FROM Users WHERE UserId = @CreatedBy),
                GETUTCDATE());
        
        COMMIT TRANSACTION;
        
        SELECT @NewProductId AS ProductId;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Procedure: Update Product
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_UpdateProduct')
    DROP PROCEDURE sp_UpdateProduct;
GO

CREATE PROCEDURE sp_UpdateProduct
    @ProductId INT,
    @ProductName NVARCHAR(200),
    @ProductCode NVARCHAR(50),
    @Description NVARCHAR(1000) = NULL,
    @Category NVARCHAR(100),
    @Price DECIMAL(18,2),
    @CostPrice DECIMAL(18,2) = NULL,
    @Quantity INT,
    @MinimumStock INT,
    @SKU NVARCHAR(100) = NULL,
    @Barcode NVARCHAR(100) = NULL,
    @Weight DECIMAL(10,3) = NULL,
    @Dimensions NVARCHAR(100) = NULL,
    @Color NVARCHAR(50) = NULL,
    @Size NVARCHAR(50) = NULL,
    @Brand NVARCHAR(100) = NULL,
    @Supplier NVARCHAR(200) = NULL,
    @IsActive BIT,
    @ModifiedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @OldValues NVARCHAR(MAX);
    DECLARE @NewValues NVARCHAR(MAX);
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if product exists
        IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductId = @ProductId)
        BEGIN
            RAISERROR('Product not found', 16, 1);
            RETURN -1;
        END
        
        -- Check if product code already exists for different product
        IF EXISTS (SELECT 1 FROM Products WHERE ProductCode = @ProductCode AND ProductId != @ProductId)
        BEGIN
            RAISERROR('Product code already exists', 16, 1);
            RETURN -2;
        END
        
        -- Check if SKU already exists for different product (if provided)
        IF @SKU IS NOT NULL AND EXISTS (SELECT 1 FROM Products WHERE SKU = @SKU AND ProductId != @ProductId)
        BEGIN
            RAISERROR('SKU already exists', 16, 1);
            RETURN -3;
        END
        
        -- Get old values for audit
        SELECT @OldValues = CONCAT(
            'ProductName: ', ProductName, ', ProductCode: ', ProductCode, 
            ', Category: ', Category, ', Price: ', CAST(Price AS NVARCHAR(20)),
            ', Quantity: ', CAST(Quantity AS NVARCHAR(10)), ', IsActive: ', CAST(IsActive AS NVARCHAR(10))
        )
        FROM Products WHERE ProductId = @ProductId;
        
        -- Update product
        UPDATE Products 
        SET ProductName = @ProductName,
            ProductCode = @ProductCode,
            Description = @Description,
            Category = @Category,
            Price = @Price,
            CostPrice = @CostPrice,
            Quantity = @Quantity,
            MinimumStock = @MinimumStock,
            SKU = @SKU,
            Barcode = @Barcode,
            Weight = @Weight,
            Dimensions = @Dimensions,
            Color = @Color,
            Size = @Size,
            Brand = @Brand,
            Supplier = @Supplier,
            IsActive = @IsActive,
            LastModifiedDate = GETUTCDATE(),
            LastModifiedBy = @ModifiedBy
        WHERE ProductId = @ProductId;
        
        -- Set new values for audit
        SET @NewValues = CONCAT(
            'ProductName: ', @ProductName, ', ProductCode: ', @ProductCode, 
            ', Category: ', @Category, ', Price: ', CAST(@Price AS NVARCHAR(20)),
            ', Quantity: ', CAST(@Quantity AS NVARCHAR(10)), ', IsActive: ', CAST(@IsActive AS NVARCHAR(10))
        );
        
        -- Log audit trail
        INSERT INTO AuditLog (TableName, RecordId, Action, OldValues, NewValues, UserId, Username, Timestamp)
        VALUES ('Products', CAST(@ProductId AS NVARCHAR(50)), 'UPDATE', @OldValues, @NewValues,
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

-- Procedure: Delete Product (Soft Delete)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_DeleteProduct')
    DROP PROCEDURE sp_DeleteProduct;
GO

CREATE PROCEDURE sp_DeleteProduct
    @ProductId INT,
    @DeletedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @OldValues NVARCHAR(MAX);
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if product exists and is active
        IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductId = @ProductId AND IsActive = 1)
        BEGIN
            RAISERROR('Active product not found', 16, 1);
            RETURN -1;
        END
        
        -- Get old values for audit
        SELECT @OldValues = CONCAT(
            'ProductName: ', ProductName, ', ProductCode: ', ProductCode, 
            ', Category: ', Category, ', Price: ', CAST(Price AS NVARCHAR(20))
        )
        FROM Products WHERE ProductId = @ProductId;
        
        -- Soft delete product
        UPDATE Products 
        SET IsActive = 0,
            LastModifiedDate = GETUTCDATE(),
            LastModifiedBy = @DeletedBy
        WHERE ProductId = @ProductId;
        
        -- Log audit trail
        INSERT INTO AuditLog (TableName, RecordId, Action, OldValues, UserId, Username, Timestamp)
        VALUES ('Products', CAST(@ProductId AS NVARCHAR(50)), 'DELETE', @OldValues,
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
-- 2. PRODUCT INVENTORY PROCEDURES
-- =====================================================

-- Procedure: Update Stock Quantity
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_UpdateProductStock')
    DROP PROCEDURE sp_UpdateProductStock;
GO

CREATE PROCEDURE sp_UpdateProductStock
    @ProductId INT,
    @QuantityChange INT, -- Positive for adding stock, negative for reducing
    @Reason NVARCHAR(200) = NULL,
    @ModifiedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @OldQuantity INT;
    DECLARE @NewQuantity INT;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Get current quantity
        SELECT @OldQuantity = Quantity
        FROM Products 
        WHERE ProductId = @ProductId AND IsActive = 1;
        
        IF @OldQuantity IS NULL
        BEGIN
            RAISERROR('Product not found or inactive', 16, 1);
            RETURN -1;
        END
        
        SET @NewQuantity = @OldQuantity + @QuantityChange;
        
        -- Prevent negative stock
        IF @NewQuantity < 0
        BEGIN
            RAISERROR('Insufficient stock. Current quantity: %d, Requested change: %d', 16, 1, @OldQuantity, @QuantityChange);
            RETURN -2;
        END
        
        -- Update stock
        UPDATE Products 
        SET Quantity = @NewQuantity,
            LastModifiedDate = GETUTCDATE(),
            LastModifiedBy = @ModifiedBy
        WHERE ProductId = @ProductId;
        
        -- Log audit trail
        INSERT INTO AuditLog (TableName, RecordId, Action, OldValues, NewValues, UserId, Username, Timestamp)
        VALUES ('Products', CAST(@ProductId AS NVARCHAR(50)), 'UPDATE', 
                'Stock Change - Old Quantity: ' + CAST(@OldQuantity AS NVARCHAR(10)),
                'New Quantity: ' + CAST(@NewQuantity AS NVARCHAR(10)) + 
                CASE WHEN @Reason IS NOT NULL THEN ', Reason: ' + @Reason ELSE '' END,
                @ModifiedBy, 
                (SELECT Username FROM Users WHERE UserId = @ModifiedBy),
                GETUTCDATE());
        
        COMMIT TRANSACTION;
        
        SELECT @NewQuantity AS NewQuantity;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Procedure: Get Low Stock Products
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetLowStockProducts')
    DROP PROCEDURE sp_GetLowStockProducts;
GO

CREATE PROCEDURE sp_GetLowStockProducts
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ProductId,
        ProductName,
        ProductCode,
        Category,
        Quantity,
        MinimumStock,
        (MinimumStock - Quantity) AS ShortfallQuantity,
        Brand,
        Supplier
    FROM Products 
    WHERE IsActive = 1 
          AND Quantity <= MinimumStock
    ORDER BY (MinimumStock - Quantity) DESC;
END
GO

-- Procedure: Get Product Categories
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetProductCategories')
    DROP PROCEDURE sp_GetProductCategories;
GO

CREATE PROCEDURE sp_GetProductCategories
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Category,
        COUNT(*) AS ProductCount,
        SUM(Quantity) AS TotalQuantity,
        AVG(Price) AS AveragePrice,
        MIN(Price) AS MinPrice,
        MAX(Price) AS MaxPrice
    FROM Products 
    WHERE IsActive = 1
    GROUP BY Category
    ORDER BY Category;
END
GO

-- Procedure: Get Product Statistics
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetProductStatistics')
    DROP PROCEDURE sp_GetProductStatistics;
GO

CREATE PROCEDURE sp_GetProductStatistics
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        COUNT(*) AS TotalProducts,
        COUNT(CASE WHEN IsActive = 1 THEN 1 END) AS ActiveProducts,
        COUNT(CASE WHEN IsActive = 0 THEN 1 END) AS InactiveProducts,
        COUNT(CASE WHEN Quantity = 0 THEN 1 END) AS OutOfStockProducts,
        COUNT(CASE WHEN Quantity <= MinimumStock AND Quantity > 0 THEN 1 END) AS LowStockProducts,
        SUM(CASE WHEN IsActive = 1 THEN Quantity ELSE 0 END) AS TotalQuantityInStock,
        AVG(CASE WHEN IsActive = 1 THEN Price ELSE NULL END) AS AveragePrice,
        COUNT(DISTINCT Category) AS TotalCategories,
        COUNT(DISTINCT Brand) AS TotalBrands
    FROM Products;
END
GO

PRINT 'Product management stored procedures created successfully!';
PRINT 'Procedures: CRUD operations, Inventory management, Statistics';
GO