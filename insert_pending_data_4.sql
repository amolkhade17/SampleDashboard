-- Insert Pending Records
INSERT INTO PendingRecords (RecordType, Operation, RecordData, Status, MakerId, MakerName, CreatedDate) 
VALUES 
('User', 'Create', '{"Username":"john.doe","Email":"john@example.com","FullName":"John Doe","RoleId":2}', 'Pending', 1, 'admin', GETDATE()),
('Product', 'Update', '{"ProductId":1,"ProductName":"Updated Laptop","Price":1299.99}', 'Pending', 1, 'admin', GETDATE()),
('User', 'Delete', '{"UserId":5,"Username":"old.user"}', 'Pending', 6, 'maker', GETDATE()),
('Product', 'Create', '{"ProductName":"New Tablet","Category":"Electronics","Price":599.99}', 'Pending', 6, 'maker', GETDATE()),
('User', 'Update', '{"UserId":3,"Username":"jane.smith","Email":"jane.new@example.com"}', 'Pending', 1, 'admin', GETDATE());

-- Insert Approved Records
INSERT INTO PendingRecords (RecordType, Operation, RecordData, Status, MakerId, MakerName, CheckerId, CheckerName, CheckerComments, CreatedDate, AuthorizedDate) 
VALUES 
('User', 'Create', '{"Username":"alice.wonder","Email":"alice@example.com","FullName":"Alice Wonder","RoleId":3}', 'Approved', 1, 'admin', 7, 'checker', 'Approved - Valid user details', DATEADD(day, -2, GETDATE()), DATEADD(day, -1, GETDATE())),
('Product', 'Update', '{"ProductId":2,"ProductName":"Premium Mouse","Price":49.99}', 'Approved', 6, 'maker', 7, 'checker', 'Price update approved', DATEADD(day, -3, GETDATE()), DATEADD(day, -2, GETDATE())),
('User', 'Update', '{"UserId":2,"IsActive":true}', 'Approved', 1, 'admin', 7, 'checker', 'User activation approved', DATEADD(day, -1, GETDATE()), GETDATE()),
('Product', 'Create', '{"ProductName":"Wireless Keyboard","Category":"Electronics","Price":79.99}', 'Approved', 6, 'maker', 7, 'checker', 'New product approved', DATEADD(day, -4, GETDATE()), DATEADD(day, -3, GETDATE())),
('User', 'Delete', '{"UserId":7,"Username":"inactive.user"}', 'Approved', 1, 'admin', 7, 'checker', 'User deletion approved', DATEADD(day, -5, GETDATE()), DATEADD(day, -4, GETDATE()));

-- Insert Rejected Records
INSERT INTO PendingRecords (RecordType, Operation, RecordData, Status, MakerId, MakerName, CheckerId, CheckerName, CheckerComments, CreatedDate, AuthorizedDate) 
VALUES 
('User', 'Create', '{"Username":"invalid.user","Email":"invalid","FullName":"Invalid User","RoleId":2}', 'Rejected', 6, 'maker', 7, 'checker', 'Invalid email format', DATEADD(day, -3, GETDATE()), DATEADD(day, -2, GETDATE())),
('Product', 'Update', '{"ProductId":5,"Price":-100}', 'Rejected', 1, 'admin', 7, 'checker', 'Invalid price - cannot be negative', DATEADD(day, -4, GETDATE()), DATEADD(day, -3, GETDATE())),
('User', 'Delete', '{"UserId":1,"Username":"admin"}', 'Rejected', 6, 'maker', 7, 'checker', 'Cannot delete admin user', DATEADD(day, -2, GETDATE()), DATEADD(day, -1, GETDATE())),
('Product', 'Delete', '{"ProductId":1}', 'Rejected', 6, 'maker', 7, 'checker', 'Product has active orders', DATEADD(day, -6, GETDATE()), DATEADD(day, -5, GETDATE())),
('User', 'Update', '{"UserId":2,"RoleId":1}', 'Rejected', 6, 'maker', 7, 'checker', 'Role elevation requires security review', DATEADD(day, -1, GETDATE()), GETDATE());

-- Display inserted records
SELECT 
    PendingId,
    RecordType,
    Operation,
    Status,
    MakerName,
    CheckerName,
    CONVERT(varchar, CreatedDate, 120) as Created,
    CONVERT(varchar, AuthorizedDate, 120) as Authorized
FROM PendingRecords
ORDER BY CreatedDate DESC;
