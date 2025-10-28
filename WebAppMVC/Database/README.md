# MSSQL Database Scripts for ASP.NET Core 9 MVC Web Application

This folder contains comprehensive MSSQL database scripts for the complete setup and configuration of the web application database.

## 📁 Script Files Overview

### Core Database Scripts
1. **`00_CreateDatabase.sql`** - Database creation with proper configuration
2. **`01_CreateTables.sql`** - Complete table schema with indexes and constraints
3. **`02_UserStoredProcedures_Updated.sql`** - User management and authentication procedures
4. **`03_ProductStoredProcedures.sql`** - Product CRUD and inventory management procedures
5. **`04_ApplicationStoredProcedures.sql`** - Application management and utility procedures
6. **`05_SampleData.sql`** - Sample data for testing and demonstration

### Deployment Scripts
- **`Deploy_Complete_Database.sql`** - Master deployment script (executes all scripts)

### Legacy Scripts (for reference)
- `01_CreateSchema.sql` - Original schema creation
- `02_UserStoredProcedures.sql` - Original user procedures
- `03_ApplicationStoredProcedures.sql` - Original application procedures

## 🚀 Quick Deployment

### Option 1: Master Deployment Script (Recommended)
```sql
-- Execute this single script to deploy everything
:r "Deploy_Complete_Database.sql"
```

### Option 2: Manual Step-by-Step Deployment
Execute scripts in the following order:
```sql
-- 1. Create database
:r "00_CreateDatabase.sql"

-- 2. Create tables and indexes
:r "01_CreateTables.sql"

-- 3. Create user management stored procedures
:r "02_UserStoredProcedures_Updated.sql"

-- 4. Create product management stored procedures
:r "03_ProductStoredProcedures.sql"

-- 5. Create application and utility stored procedures
:r "04_ApplicationStoredProcedures.sql"

-- 6. Insert sample data (optional)
:r "05_SampleData.sql"
```

## 📊 Database Schema

### Tables Created
1. **Users** - User accounts, authentication, and profiles
2. **Applications** - Application modules and navigation
3. **CrudOperations** - CRUD operations for each application
4. **Products** - Product catalog and inventory
5. **AuditLog** - System audit trail and change tracking
6. **UserSessions** - JWT session management

### Key Features
- ✅ Foreign key relationships and constraints
- ✅ Indexes for optimal performance
- ✅ Audit trail for all changes
- ✅ Soft delete functionality
- ✅ Role-based access control
- ✅ Session management for JWT tokens

## 🔧 Stored Procedures

### User Management (31 procedures)
- Authentication and login
- User CRUD operations
- Session management
- Password handling

### Product Management (12 procedures)
- Product CRUD operations
- Inventory management
- Stock tracking
- Category management

### Application Management (8 procedures)
- Application configuration
- CRUD operation definitions
- Audit reporting
- System statistics

### Utility Procedures (5 procedures)
- Global search functionality
- Database health checks
- Performance monitoring
- Data cleanup

## 🔐 Sample Login Credentials

The sample data script creates the following test accounts:

| Username | Password | Role | Description |
|----------|----------|------|-------------|
| `admin` | `admin123` | Admin | System administrator |
| `superadmin` | `super123` | Admin | Super administrator |
| `manager1` | `manager123` | Manager | Sales manager |
| `manager2` | `manager123` | Manager | Marketing manager |
| `user1` | `user123` | User | Regular user (Sales) |
| `user2` | `user123` | User | Regular user (Marketing) |
| `user3` | `user123` | User | Regular user (IT) |
| `demo` | `demo123` | User | Demo account |
| `testuser` | `test123` | User | Test account |

## 🔗 Connection String

Update your `appsettings.json` with:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local);Database=WebAppMVCDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

For SQL Server Authentication:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SERVER_NAME;Database=WebAppMVCDb;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=true;"
  }
}
```

## 📦 Sample Data Included

### Applications (8 modules)
- User Management
- Product Management  
- Order Management
- Report Center
- Settings & Configuration
- Dashboard Analytics
- File Manager
- Audit & Compliance

### Products (15 items across 5 categories)
- Electronics (3 items)
- Clothing (3 items)
- Home & Garden (3 items)
- Books (3 items)
- Sports & Fitness (3 items)

### CRUD Operations (25 operations)
- Comprehensive CRUD operations for each application
- Role-based access control
- Proper navigation and routing

## 🛠️ Performance Optimizations

### Indexes Created
- Primary key indexes (automatic)
- Foreign key indexes
- Search optimization indexes
- Performance query indexes

### Best Practices Implemented
- Parameterized queries prevention SQL injection
- Proper transaction handling
- Error handling and rollback
- Audit trail for all changes
- Soft delete for data integrity

## 🔍 Testing the Deployment

After deployment, verify with these queries:

```sql
-- Check table creation
SELECT TABLE_NAME, TABLE_ROWS 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo';

-- Check stored procedures
SELECT ROUTINE_NAME 
FROM INFORMATION_SCHEMA.ROUTINES 
WHERE ROUTINE_TYPE = 'PROCEDURE';

-- Test sample data
SELECT COUNT(*) AS UserCount FROM Users;
SELECT COUNT(*) AS ProductCount FROM Products;
SELECT COUNT(*) AS ApplicationCount FROM Applications;

-- Test authentication procedure
EXEC sp_GetUserByUsername @Username = 'admin';
```

## 🚨 Important Notes

1. **Backup**: Always backup existing data before running deployment scripts
2. **Environment**: Test scripts in development environment first
3. **Permissions**: Ensure SQL Server login has sufficient privileges
4. **Collation**: Scripts use SQL_Latin1_General_CP1_CI_AS collation
5. **Version**: Designed for SQL Server 2016+ and ASP.NET Core 9

## 🔄 Update Scripts

To update an existing database:
1. Run individual updated procedure scripts
2. Use ALTER statements instead of DROP/CREATE for production
3. Always maintain backward compatibility
4. Test updates on development environment first

## 📞 Support

These scripts are designed to work seamlessly with the ASP.NET Core 9 MVC web application. All stored procedures match the repository patterns and models used in the application code.

For issues or modifications, refer to the application's repository classes and model definitions to ensure compatibility.