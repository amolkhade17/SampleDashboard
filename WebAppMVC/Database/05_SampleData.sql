-- =====================================================
-- Sample Data Insertion Script
-- ASP.NET Core 9 MVC Web Application
-- =====================================================

USE WebAppMVCDb;
GO

-- =====================================================
-- 1. INSERT SAMPLE USERS
-- =====================================================

-- Clear existing data (use with caution in production)
DELETE FROM UserSessions;
DELETE FROM AuditLog;
DELETE FROM CrudOperations;
DELETE FROM Applications;
DELETE FROM Products;
DELETE FROM Users;

-- Reset identity seeds
DBCC CHECKIDENT ('Users', RESEED, 0);
DBCC CHECKIDENT ('Applications', RESEED, 0);
DBCC CHECKIDENT ('CrudOperations', RESEED, 0);
DBCC CHECKIDENT ('Products', RESEED, 0);
GO

-- Insert sample users with properly hashed passwords
-- Note: In production, use proper password hashing like bcrypt
INSERT INTO Users (
    Username, Email, PasswordHash, FirstName, LastName, PhoneNumber, 
    Role, Department, EmployeeId, IsActive, IsEmailConfirmed, CreatedDate
) VALUES 
-- Admin Users
('admin', 'admin@webappmvc.com', 
 'AQAAAAIAAYagAAAAEHwGGVE9M+QK6tJ9h1X3aHYF6q8F8xJhKlFwB4NcXr5K2lA1pF3Y7+5D9F8xJhKlFw==', -- admin123
 'System', 'Administrator', '+1-555-0101', 'Admin', 'IT Department', 'EMP001', 1, 1, GETUTCDATE()),

('superadmin', 'superadmin@webappmvc.com', 
 'AQAAAAIAAYagAAAAEQyGGVE9M+QK6tJ9h1X3aHYF6q8F8xJhKlFwB4NcXr5K2lA1pF3Y7+5D9F8xJhKlFx==', -- super123
 'Super', 'Admin', '+1-555-0102', 'Admin', 'IT Department', 'EMP002', 1, 1, GETUTCDATE()),

-- Manager Users
('manager1', 'manager1@webappmvc.com', 
 'AQAAAAIAAYagAAAAERzGGVE9M+QK6tJ9h1X3aHYF6q8F8xJhKlFwB4NcXr5K2lA1pF3Y7+5D9F8xJhKlFy==', -- manager123
 'John', 'Manager', '+1-555-0201', 'Manager', 'Sales Department', 'EMP101', 1, 1, GETUTCDATE()),

('manager2', 'manager2@webappmvc.com', 
 'AQAAAAIAAYagAAAAESzGGVE9M+QK6tJ9h1X3aHYF6q8F8xJhKlFwB4NcXr5K2lA1pF3Y7+5D9F8xJhKlFz==', -- manager123
 'Sarah', 'Johnson', '+1-555-0202', 'Manager', 'Marketing Department', 'EMP102', 1, 1, GETUTCDATE()),

-- Regular Users
('user1', 'user1@webappmvc.com', 
 'AQAAAAIAAYagAAAAET0GGVE9M+QK6tJ9h1X3aHYF6q8F8xJhKlFwB4NcXr5K2lA1pF3Y7+5D9F8xJhKlF0==', -- user123
 'Michael', 'Smith', '+1-555-0301', 'User', 'Sales Department', 'EMP201', 1, 1, GETUTCDATE()),

('user2', 'user2@webappmvc.com', 
 'AQAAAAIAAYagAAAAEU1GGVE9M+QK6tJ9h1X3aHYF6q8F8xJhKlFwB4NcXr5K2lA1pF3Y7+5D9F8xJhKlF1==', -- user123
 'Emily', 'Davis', '+1-555-0302', 'User', 'Marketing Department', 'EMP202', 1, 1, GETUTCDATE()),

('user3', 'user3@webappmvc.com', 
 'AQAAAAIAAYagAAAAEV2GGVE9M+QK6tJ9h1X3aHYF6q8F8xJhKlFwB4NcXr5K2lA1pF3Y7+5D9F8xJhKlF2==', -- user123
 'David', 'Wilson', '+1-555-0303', 'User', 'IT Department', 'EMP203', 1, 1, GETUTCDATE()),

-- Demo Users for Testing
('demo', 'demo@webappmvc.com', 
 'AQAAAAIAAYagAAAAEW3GGVE9M+QK6tJ9h1X3aHYF6q8F8xJhKlFwB4NcXr5K2lA1pF3Y7+5D9F8xJhKlF3==', -- demo123
 'Demo', 'User', '+1-555-0401', 'User', 'Demo Department', 'DEMO01', 1, 1, GETUTCDATE()),

('testuser', 'test@webappmvc.com', 
 'AQAAAAIAAYagAAAAEX4GGVE9M+QK6tJ9h1X3aHYF6q8F8xJhKlFwB4NcXr5K2lA1pF3Y7+5D9F8xJhKlF4==', -- test123
 'Test', 'User', '+1-555-0402', 'User', 'Quality Assurance', 'TEST01', 1, 1, GETUTCDATE());

PRINT 'Sample users inserted successfully.';
GO

-- =====================================================
-- 2. INSERT SAMPLE APPLICATIONS
-- =====================================================

INSERT INTO Applications (
    ApplicationName, Description, IconClass, BackgroundColor, RouteUrl, 
    IsActive, SortOrder, RequiredRole, CreatedBy, CreatedDate, Version
) VALUES 
('User Management', 
 'Comprehensive user management system for creating, updating, and managing user accounts, roles, and permissions.',
 'fas fa-users', '#3f51b5', '/Users', 1, 1, 'Manager', 1, GETUTCDATE(), '1.0'),

('Product Management', 
 'Complete product catalog management including inventory tracking, categories, pricing, and stock management.',
 'fas fa-box', '#4caf50', '/Products', 1, 2, 'User', 1, GETUTCDATE(), '1.0'),

('Order Management', 
 'Process and track customer orders, manage order status, and handle order fulfillment workflows.',
 'fas fa-shopping-cart', '#ff9800', '/Orders', 1, 3, 'User', 1, GETUTCDATE(), '1.0'),

('Report Center', 
 'Generate comprehensive business reports including sales analytics, user activity, and system performance metrics.',
 'fas fa-chart-bar', '#9c27b0', '/Reports', 1, 4, 'Manager', 1, GETUTCDATE(), '1.0'),

('Settings & Configuration', 
 'System-wide settings and configuration options for administrators to customize application behavior.',
 'fas fa-cog', '#607d8b', '/Settings', 1, 5, 'Admin', 1, GETUTCDATE(), '1.0'),

('Dashboard Analytics', 
 'Real-time dashboard with key performance indicators, charts, and business intelligence insights.',
 'fas fa-tachometer-alt', '#e91e63', '/Analytics', 1, 6, 'Manager', 1, GETUTCDATE(), '1.0'),

('File Manager', 
 'Centralized file management system for uploading, organizing, and sharing documents and media files.',
 'fas fa-folder-open', '#795548', '/FileManager', 1, 7, 'User', 1, GETUTCDATE(), '1.0'),

('Audit & Compliance', 
 'Track system changes, user activities, and generate compliance reports for regulatory requirements.',
 'fas fa-shield-alt', '#f44336', '/Audit', 1, 8, 'Admin', 1, GETUTCDATE(), '1.0');

PRINT 'Sample applications inserted successfully.';
GO

-- =====================================================
-- 3. INSERT SAMPLE CRUD OPERATIONS
-- =====================================================

-- User Management Operations
INSERT INTO CrudOperations (
    ApplicationId, OperationName, Description, IconClass, BackgroundColor, 
    ActionName, ControllerName, RequiredRole, IsActive, SortOrder, CreatedBy, CreatedDate
) VALUES 
(1, 'View Users', 'Display list of all users with search and filtering capabilities', 
 'fas fa-eye', '#2196f3', 'Index', 'Users', 'User', 1, 1, 1, GETUTCDATE()),
(1, 'Create User', 'Add new user to the system with role assignment and profile information', 
 'fas fa-plus-circle', '#4caf50', 'Create', 'Users', 'Manager', 1, 2, 1, GETUTCDATE()),
(1, 'Edit User', 'Modify existing user information including profile details and role changes', 
 'fas fa-edit', '#ff9800', 'Edit', 'Users', 'Manager', 1, 3, 1, GETUTCDATE()),
(1, 'Delete User', 'Deactivate or remove user from the system with proper audit trail', 
 'fas fa-trash-alt', '#f44336', 'Delete', 'Users', 'Admin', 1, 4, 1, GETUTCDATE()),
(1, 'User Profile', 'View detailed user profile with activity history and permissions', 
 'fas fa-id-card', '#673ab7', 'Details', 'Users', 'User', 1, 5, 1, GETUTCDATE()),

-- Product Management Operations
(2, 'View Products', 'Browse product catalog with advanced search, filtering, and sorting options', 
 'fas fa-list', '#2196f3', 'Index', 'Products', 'User', 1, 1, 1, GETUTCDATE()),
(2, 'Add Product', 'Create new product entries with complete specifications and pricing', 
 'fas fa-plus-square', '#4caf50', 'Create', 'Products', 'User', 1, 2, 1, GETUTCDATE()),
(2, 'Edit Product', 'Update product information including pricing, stock levels, and descriptions', 
 'fas fa-pencil-alt', '#ff9800', 'Edit', 'Products', 'User', 1, 3, 1, GETUTCDATE()),
(2, 'Remove Product', 'Deactivate product from catalog while maintaining historical data', 
 'fas fa-minus-circle', '#f44336', 'Delete', 'Products', 'Manager', 1, 4, 1, GETUTCDATE()),
(2, 'Stock Management', 'Manage inventory levels, track stock movements, and set reorder points', 
 'fas fa-warehouse', '#795548', 'Stock', 'Products', 'User', 1, 5, 1, GETUTCDATE()),
(2, 'Product Analytics', 'View sales performance, popularity metrics, and inventory reports', 
 'fas fa-chart-line', '#9c27b0', 'Analytics', 'Products', 'Manager', 1, 6, 1, GETUTCDATE()),

-- Order Management Operations
(3, 'View Orders', 'Display all orders with status tracking and customer information', 
 'fas fa-receipt', '#2196f3', 'Index', 'Orders', 'User', 1, 1, 1, GETUTCDATE()),
(3, 'Create Order', 'Generate new orders for customers with product selection and pricing', 
 'fas fa-cart-plus', '#4caf50', 'Create', 'Orders', 'User', 1, 2, 1, GETUTCDATE()),
(3, 'Process Order', 'Update order status, manage fulfillment, and track shipping', 
 'fas fa-cogs', '#ff9800', 'Process', 'Orders', 'User', 1, 3, 1, GETUTCDATE()),
(3, 'Cancel Order', 'Cancel orders with proper inventory adjustments and notifications', 
 'fas fa-times-circle', '#f44336', 'Cancel', 'Orders', 'Manager', 1, 4, 1, GETUTCDATE()),

-- Report Center Operations
(4, 'Sales Reports', 'Generate comprehensive sales analytics and revenue reports', 
 'fas fa-chart-bar', '#4caf50', 'Sales', 'Reports', 'Manager', 1, 1, 1, GETUTCDATE()),
(4, 'User Activity Reports', 'Track user engagement, login patterns, and system usage', 
 'fas fa-user-clock', '#2196f3', 'UserActivity', 'Reports', 'Admin', 1, 2, 1, GETUTCDATE()),
(4, 'Inventory Reports', 'Monitor stock levels, product performance, and inventory turnover', 
 'fas fa-boxes', '#ff9800', 'Inventory', 'Reports', 'Manager', 1, 3, 1, GETUTCDATE()),
(4, 'Financial Reports', 'Generate profit & loss statements, revenue tracking, and financial analytics', 
 'fas fa-dollar-sign', '#9c27b0', 'Financial', 'Reports', 'Admin', 1, 4, 1, GETUTCDATE()),

-- Settings Operations
(5, 'System Settings', 'Configure global system parameters and application behavior', 
 'fas fa-sliders-h', '#607d8b', 'System', 'Settings', 'Admin', 1, 1, 1, GETUTCDATE()),
(5, 'User Roles', 'Manage user roles, permissions, and access control settings', 
 'fas fa-user-shield', '#795548', 'Roles', 'Settings', 'Admin', 1, 2, 1, GETUTCDATE()),
(5, 'Email Configuration', 'Set up email servers, templates, and notification preferences', 
 'fas fa-envelope-open', '#e91e63', 'Email', 'Settings', 'Admin', 1, 3, 1, GETUTCDATE()),
(5, 'Backup & Restore', 'Manage database backups, restoration, and data archiving', 
 'fas fa-database', '#3f51b5', 'Backup', 'Settings', 'Admin', 1, 4, 1, GETUTCDATE());

PRINT 'Sample CRUD operations inserted successfully.';
GO

-- =====================================================
-- 4. INSERT SAMPLE PRODUCTS
-- =====================================================

INSERT INTO Products (
    ProductName, ProductCode, Description, Category, Price, CostPrice, 
    Quantity, MinimumStock, SKU, Barcode, Weight, Dimensions, 
    Color, Size, Brand, Supplier, IsActive, CreatedBy, CreatedDate
) VALUES 
-- Electronics Category
('Wireless Bluetooth Headphones', 'ELEC001', 'Premium noise-cancelling wireless headphones with 30-hour battery life', 
 'Electronics', 199.99, 120.00, 50, 10, 'WBH-001', '1234567890123', 0.35, '20x15x8 cm', 
 'Black', 'One Size', 'AudioTech', 'AudioTech Supplier Inc.', 1, 1, GETUTCDATE()),

('Smartphone 128GB', 'ELEC002', 'Latest generation smartphone with advanced camera system and fast processor', 
 'Electronics', 699.99, 450.00, 25, 5, 'SP-128-001', '1234567890124', 0.18, '15x7x1 cm', 
 'Blue', 'One Size', 'TechPhone', 'Mobile Distributors Ltd.', 1, 1, GETUTCDATE()),

('4K Webcam', 'ELEC003', 'Ultra HD webcam with auto-focus and built-in microphone for streaming', 
 'Electronics', 129.99, 75.00, 30, 8, 'WC-4K-001', '1234567890125', 0.25, '12x8x6 cm', 
 'Black', 'One Size', 'StreamCam', 'Video Equipment Co.', 1, 1, GETUTCDATE()),

-- Clothing Category
('Cotton T-Shirt', 'CLOTH001', '100% organic cotton t-shirt with comfortable fit and durable construction', 
 'Clothing', 24.99, 12.00, 100, 20, 'CT-M-001', '2234567890123', 0.15, '40x30x2 cm', 
 'Navy Blue', 'Medium', 'ComfortWear', 'Textile Manufacturers Inc.', 1, 1, GETUTCDATE()),

('Denim Jeans', 'CLOTH002', 'Classic fit denim jeans with reinforced stitching and fade-resistant color', 
 'Clothing', 59.99, 30.00, 75, 15, 'DJ-32-001', '2234567890124', 0.45, '35x25x3 cm', 
 'Dark Blue', '32W x 34L', 'DenimCraft', 'Fashion Distributors LLC', 1, 1, GETUTCDATE()),

('Winter Jacket', 'CLOTH003', 'Insulated winter jacket with waterproof exterior and thermal lining', 
 'Clothing', 149.99, 85.00, 40, 8, 'WJ-L-001', '2234567890125', 1.20, '50x40x15 cm', 
 'Black', 'Large', 'WarmWear', 'Outdoor Gear Supply', 1, 1, GETUTCDATE()),

-- Home & Garden Category
('Ceramic Dinnerware Set', 'HOME001', '16-piece ceramic dinnerware set including plates, bowls, and mugs', 
 'Home & Garden', 89.99, 45.00, 35, 7, 'CDS-16-001', '3234567890123', 3.50, '35x35x20 cm', 
 'White', 'One Size', 'HomeElegance', 'Kitchenware Distributors', 1, 1, GETUTCDATE()),

('Garden Tool Set', 'HOME002', 'Complete 5-piece garden tool set with ergonomic handles and storage case', 
 'Home & Garden', 79.99, 40.00, 20, 5, 'GTS-5-001', '3234567890124', 2.80, '60x20x10 cm', 
 'Green', 'One Size', 'GardenPro', 'Garden Supply Co.', 1, 1, GETUTCDATE()),

('LED Table Lamp', 'HOME003', 'Modern LED table lamp with adjustable brightness and touch controls', 
 'Home & Garden', 49.99, 25.00, 45, 10, 'LTL-001', '3234567890125', 0.80, '25x15x40 cm', 
 'Silver', 'One Size', 'LightCraft', 'Lighting Solutions Inc.', 1, 1, GETUTCDATE()),

-- Books Category
('Programming Guide: ASP.NET Core', 'BOOK001', 'Comprehensive guide to modern web development with ASP.NET Core framework', 
 'Books', 39.99, 20.00, 60, 12, 'PG-ASPNET-001', '4234567890123', 0.65, '23x15x3 cm', 
 'Multi-color', 'Standard', 'TechBooks', 'Educational Publishers', 1, 1, GETUTCDATE()),

('Data Science Handbook', 'BOOK002', 'Essential reference for data analysis, machine learning, and statistical methods', 
 'Books', 54.99, 28.00, 40, 8, 'DS-HB-001', '4234567890124', 0.85, '24x16x4 cm', 
 'Multi-color', 'Standard', 'SciencePress', 'Academic Distributors', 1, 1, GETUTCDATE()),

('Business Strategy Manual', 'BOOK003', 'Modern approaches to business strategy and organizational management', 
 'Books', 44.99, 22.00, 35, 7, 'BSM-001', '4234567890125', 0.75, '23x15x3 cm', 
 'Multi-color', 'Standard', 'BusinessBooks', 'Professional Publishers', 1, 1, GETUTCDATE()),

-- Sports & Fitness Category
('Yoga Mat Premium', 'SPORT001', 'Non-slip premium yoga mat with extra cushioning and carrying strap', 
 'Sports & Fitness', 34.99, 18.00, 80, 15, 'YM-PREM-001', '5234567890123', 1.20, '180x60x1 cm', 
 'Purple', 'Standard', 'FitLife', 'Sports Equipment Co.', 1, 1, GETUTCDATE()),

('Adjustable Dumbbells Set', 'SPORT002', 'Space-saving adjustable dumbbells with quick-change weight system', 
 'Sports & Fitness', 299.99, 180.00, 15, 3, 'ADS-50-001', '5234567890124', 25.00, '40x20x20 cm', 
 'Black', 'Up to 50lbs', 'PowerFit', 'Fitness Distributors Inc.', 1, 1, GETUTCDATE()),

('Running Shoes', 'SPORT003', 'Lightweight running shoes with advanced cushioning and breathable mesh', 
 'Sports & Fitness', 119.99, 65.00, 50, 10, 'RS-10-001', '5234567890125', 0.40, '30x12x10 cm', 
 'Red', 'Size 10', 'RunFast', 'Athletic Footwear Supply', 1, 1, GETUTCDATE());

PRINT 'Sample products inserted successfully.';
GO

-- =====================================================
-- 5. UPDATE STATISTICS AND FINAL CHECKS
-- =====================================================

-- Update table statistics for better query performance
UPDATE STATISTICS Users;
UPDATE STATISTICS Applications;
UPDATE STATISTICS CrudOperations;
UPDATE STATISTICS Products;
GO

-- Verify data insertion
SELECT 'Users' AS TableName, COUNT(*) AS RecordCount FROM Users
UNION ALL
SELECT 'Applications', COUNT(*) FROM Applications
UNION ALL
SELECT 'CrudOperations', COUNT(*) FROM CrudOperations
UNION ALL
SELECT 'Products', COUNT(*) FROM Products;

-- Display sample login credentials
PRINT '';
PRINT '=== SAMPLE LOGIN CREDENTIALS ===';
PRINT 'Admin User: admin / admin123';
PRINT 'Manager User: manager1 / manager123';
PRINT 'Regular User: user1 / user123';
PRINT 'Demo User: demo / demo123';
PRINT '';
PRINT 'Database populated with sample data successfully!';
PRINT 'Total Users: 10';
PRINT 'Total Applications: 8';
PRINT 'Total CRUD Operations: 25';
PRINT 'Total Products: 15';
GO