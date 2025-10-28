-- =====================================================
-- Add Missing Columns to Users Table
-- ASP.NET Core 9 MVC Web Application
-- =====================================================

USE WebAppMVCDb;
GO

PRINT 'Adding missing columns to Users table...';

-- Add PhoneNumber column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'PhoneNumber')
BEGIN
    ALTER TABLE Users ADD PhoneNumber NVARCHAR(20) NULL;
    PRINT 'Added PhoneNumber column';
END
ELSE
BEGIN
    PRINT 'PhoneNumber column already exists';
END

-- Add IsEmailConfirmed column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'IsEmailConfirmed')
BEGIN
    ALTER TABLE Users ADD IsEmailConfirmed BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsEmailConfirmed column';
END
ELSE
BEGIN
    PRINT 'IsEmailConfirmed column already exists';
END

-- Add LastModifiedDate column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'LastModifiedDate')
BEGIN
    ALTER TABLE Users ADD LastModifiedDate DATETIME2(7) NULL;
    PRINT 'Added LastModifiedDate column';
END
ELSE
BEGIN
    PRINT 'LastModifiedDate column already exists';
END

-- Add ProfileImageUrl column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProfileImageUrl')
BEGIN
    ALTER TABLE Users ADD ProfileImageUrl NVARCHAR(500) NULL;
    PRINT 'Added ProfileImageUrl column';
END
ELSE
BEGIN
    PRINT 'ProfileImageUrl column already exists';
END

-- Add Department column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Department')
BEGIN
    ALTER TABLE Users ADD Department NVARCHAR(100) NULL;
    PRINT 'Added Department column';
END
ELSE
BEGIN
    PRINT 'Department column already exists';
END

-- Add EmployeeId column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'EmployeeId')
BEGIN
    ALTER TABLE Users ADD EmployeeId NVARCHAR(50) NULL;
    PRINT 'Added EmployeeId column';
END
ELSE
BEGIN
    PRINT 'EmployeeId column already exists';
END

-- Add CreatedBy column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'CreatedBy')
BEGIN
    ALTER TABLE Users ADD CreatedBy INT NULL;
    PRINT 'Added CreatedBy column';
END
ELSE
BEGIN
    PRINT 'CreatedBy column already exists';
END

-- Add LastModifiedBy column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'LastModifiedBy')
BEGIN
    ALTER TABLE Users ADD LastModifiedBy INT NULL;
    PRINT 'Added LastModifiedBy column';
END
ELSE
BEGIN
    PRINT 'LastModifiedBy column already exists';
END

-- Add foreign key constraints after columns are added
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_CreatedBy')
BEGIN
    ALTER TABLE Users ADD CONSTRAINT FK_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId);
    PRINT 'Added FK_Users_CreatedBy foreign key constraint';
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_ModifiedBy')
BEGIN
    ALTER TABLE Users ADD CONSTRAINT FK_Users_ModifiedBy FOREIGN KEY (LastModifiedBy) REFERENCES Users(UserId);
    PRINT 'Added FK_Users_ModifiedBy foreign key constraint';
END

-- Add indexes for performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_PhoneNumber')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_PhoneNumber ON Users (PhoneNumber);
    PRINT 'Added IX_Users_PhoneNumber index';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Department')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_Department ON Users (Department);
    PRINT 'Added IX_Users_Department index';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_EmployeeId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_EmployeeId ON Users (EmployeeId);
    PRINT 'Added IX_Users_EmployeeId index';
END

-- Add check constraints
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Users_Email_Format')
BEGIN
    ALTER TABLE Users ADD CONSTRAINT CK_Users_Email_Format CHECK (Email LIKE '%@%.%');
    PRINT 'Added CK_Users_Email_Format check constraint';
END

-- Update existing users with default values for new columns (done separately to avoid column reference issues)
PRINT 'Updating existing users with default values...';

-- Update IsEmailConfirmed
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'IsEmailConfirmed')
BEGIN
    UPDATE Users SET IsEmailConfirmed = 1 WHERE IsEmailConfirmed = 0;
    PRINT 'Updated IsEmailConfirmed for existing users';
END

-- Update Department
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Department')
BEGIN
    UPDATE Users SET Department = 'General' WHERE Department IS NULL;
    PRINT 'Updated Department for existing users';
END

-- Update EmployeeId
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'EmployeeId')
BEGIN
    UPDATE Users SET EmployeeId = 'EMP' + RIGHT('000' + CAST(UserId AS VARCHAR(3)), 3) WHERE EmployeeId IS NULL;
    PRINT 'Updated EmployeeId for existing users';
END

-- Display final table structure
PRINT '';
PRINT 'Final Users table structure:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT 'Missing columns added successfully to Users table!';
GO