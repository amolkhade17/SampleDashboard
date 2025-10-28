-- =====================================================
-- Test Script for Fixed User Stored Procedures
-- ASP.NET Core 9 MVC Web Application
-- =====================================================

USE WebAppMVCDb;
GO

PRINT 'Testing User Stored Procedures...';
PRINT '====================================';

-- Test 1: Get User by Username
PRINT 'Test 1: sp_GetUserByUsername';
EXEC sp_GetUserByUsername @Username = 'admin';

-- Test 2: Get User by Email  
PRINT 'Test 2: sp_GetUserByEmail';
EXEC sp_GetUserByEmail @Email = 'admin@webappmvc.com';

-- Test 3: Authenticate User
PRINT 'Test 3: sp_AuthenticateUser';
EXEC sp_AuthenticateUser @Username = 'admin', @Password = 'admin123';

-- Test 4: Get All Users
PRINT 'Test 4: sp_GetAllUsers';
EXEC sp_GetAllUsers;

-- Test 5: Check if user exists
PRINT 'Test 5: sp_CheckUserExists';
EXEC sp_CheckUserExists @Username = 'admin', @Email = 'admin@webappmvc.com';

-- Test 6: Update last login
PRINT 'Test 6: sp_UpdateLastLogin';
EXEC sp_UpdateLastLogin @UserId = 1;

PRINT '';
PRINT 'All stored procedure tests completed!';
PRINT 'If no errors appeared above, the procedures are working correctly.';
GO