-- Sample Applications for Dashboard
-- Quick fix to populate the Applications table for testing

USE WebAppMVCDb;
GO

-- Insert sample applications
INSERT INTO Applications (
    ApplicationName, Description, IconClass, BackgroundColor, RouteUrl, 
    IsActive, SortOrder, CreatedDate
) VALUES 
('User Management', 
 'Manage user accounts, roles, and permissions.',
 'fas fa-users', '#3f51b5', '/Users', 1, 1, GETUTCDATE()),

('Product Management', 
 'Manage product catalog and inventory.',
 'fas fa-box', '#4caf50', '/Products', 1, 2, GETUTCDATE()),

('Order Management', 
 'Process and track customer orders.',
 'fas fa-shopping-cart', '#ff9800', '/Orders', 1, 3, GETUTCDATE()),

('Reports', 
 'Generate business reports and analytics.',
 'fas fa-chart-bar', '#9c27b0', '/Reports', 1, 4, GETUTCDATE()),

('Settings', 
 'System configuration and settings.',
 'fas fa-cog', '#607d8b', '/Settings', 1, 5, GETUTCDATE());

-- Insert sample CRUD operations for User Management (ApplicationId = 1)
INSERT INTO CrudOperations (
    ApplicationId, OperationName, Description, IconClass, BackgroundColor, 
    ActionName, ControllerName, IsActive, SortOrder, CreatedDate
) VALUES 
(1, 'View Users', 'Display list of all users', 'fas fa-eye', '#2196f3', 'Index', 'Users', 1, 1, GETUTCDATE()),
(1, 'Add User', 'Create new user account', 'fas fa-plus-circle', '#4caf50', 'Create', 'Users', 1, 2, GETUTCDATE()),
(1, 'Edit User', 'Modify user information', 'fas fa-edit', '#ff9800', 'Edit', 'Users', 1, 3, GETUTCDATE()),
(1, 'Delete User', 'Remove user from system', 'fas fa-trash-alt', '#f44336', 'Delete', 'Users', 1, 4, GETUTCDATE());

-- Verify data insertion
SELECT 'Applications' AS TableName, COUNT(*) AS RecordCount FROM Applications
UNION ALL
SELECT 'CrudOperations', COUNT(*) FROM CrudOperations;

PRINT 'Sample applications and operations inserted successfully!';
PRINT 'Dashboard should now load correctly.';
GO