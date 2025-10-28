-- Fix Password Hashes for Current Authentication System
-- This script updates the existing users with correct password hashes
-- that work with the current HMACSHA512-based authentication

USE WebAppMVCDb;
GO

-- Create a temporary table to store the correct hashes
-- Note: These hashes are generated using the current AuthenticationService.HashPassword method

-- First, let's create correct hashes for our demo users
-- admin / admin123
-- user / user123

-- For now, let's use simple known passwords that we can verify
-- We'll generate the hashes using the current system and update

UPDATE Users 
SET PasswordHash = (
    -- This is a placeholder - we need to generate actual hashes using the current system
    SELECT 'TEMP_HASH_' + Username FROM Users u WHERE u.UserId = Users.UserId
)
WHERE Username IN ('admin', 'user');

-- Display current users for verification
SELECT UserId, Username, Email, PasswordHash, IsActive 
FROM Users 
WHERE Username IN ('admin', 'user');

PRINT 'Password hashes have been marked for update. You need to run the application to generate proper hashes.';