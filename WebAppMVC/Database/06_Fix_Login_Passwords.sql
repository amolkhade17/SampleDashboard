-- =====================================================
-- IMPORTANT: Password Hash Compatibility Note
-- =====================================================
-- 
-- This script contains CORRECTED password hashes that are compatible 
-- with the current AuthenticationService which uses HMACSHA512.
-- 
-- DO NOT run the original 05_SampleData.sql as it contains ASP.NET Core 
-- Identity format password hashes that are incompatible with our current 
-- authentication system.
--
-- Use this script instead for sample data with working login credentials.
-- =====================================================

USE WebAppMVCDb;
GO

-- Update admin and user1 passwords to work with current authentication system
UPDATE Users 
SET PasswordHash = 'ZiDakTgeKFdRrI/x5YtwRg5+1uw2xIK1Ekk1fIfhrQ2ZWXmC7t9ZgsOiJ3wdcKWX+poMgZh5MPweu9YIiylpXCm92I2UfoHN+Ol2YjriuYgt9KZuTtjwqXHWQv8Q2pDc3NabbxUUTRKeDlJTDYVYyBhTd/pLSAoOw9ln5jmUN6PwrIrZIpI/QpKwHlPwmZ/Al4tkD6vw3sRdyo6imPVn+TdlTEnjiLtr9fblwps0IoRkJVCdR6gor1TTbcuO6lZg'
WHERE Username = 'admin';

UPDATE Users 
SET PasswordHash = 'NE6jw0myDo8M4sXfoXmmbjMWJtCmVd/Pn09U/0tTBjma99JV4XmWsDe84T/zu7ioVZkEtZaMyWQg6LXa8giZNKrSPtLBddHLxj1KglUNOKCFdUmeXbj9+8zTlu0KW4EtVWCFMmBMUiuXMH9c7K3NHEv3vbPwuBGJtyFNEwigFmG1DnX1FUeMsiqJjUrQXiP+C190eL8C0WCpPfGwPmACr5ejxU+ZLrihj1NdF+iA8zYD6urCN9iHEnEyy1cKdsvU'
WHERE Username = 'user1';

-- Create a simple 'user' account if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'user')
BEGIN
    INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, IsActive, CreatedDate, Role) 
    VALUES ('user', 'user@webappmvc.com', 
            'ZRR+alupoNpaMFq7TDdzt666WMWFQxNzYrBa+I1pfUdmmo2ZKflvjxERfEVc4lilORJDwoen85a8LWwbw9r0ajWiQC6G3yEaJvsl2z87YLJ2viIbzHPvTWXS/b7S2JFaxu0+baMgx2olselQo1UeVhog9lQRVdGTr3gZn+yjDDivaXKyfYB3KYNueWqw/m9KRWJqqAtYvaZVNwKMp5g2VXFQxSeU2NPyX3U/mcnnTmTXkJu4zqjPe+aBfjo24aag', 
            'Regular', 'User', 1, GETUTCDATE(), 'User');
END
ELSE
BEGIN
    UPDATE Users 
    SET PasswordHash = 'ZRR+alupoNpaMFq7TDdzt666WMWFQxNzYrBa+I1pfUdmmo2ZKflvjxERfEVc4lilORJDwoen85a8LWwbw9r0ajWiQC6G3yEaJvsl2z87YLJ2viIbzHPvTWXS/b7S2JFaxu0+baMgx2olselQo1UeVhog9lQRVdGTr3gZn+yjDDivaXKyfYB3KYNueWqw/m9KRWJqqAtYvaZVNwKMp5g2VXFQxSeU2NPyX3U/mcnnTmTXkJu4zqjPe+aBfjo24aag'
    WHERE Username = 'user';
END

-- Verify the updates
SELECT Username, 'Password Updated - Compatible with HMACSHA512' as Status 
FROM Users 
WHERE Username IN ('admin', 'user', 'user1');

PRINT '';
PRINT '=== WORKING LOGIN CREDENTIALS ===';
PRINT 'Admin: admin / admin123';
PRINT 'User: user / user123';
PRINT 'User1: user1 / user123';
PRINT '';
PRINT 'Password hashes updated to HMACSHA512 format - Login should now work!';

GO