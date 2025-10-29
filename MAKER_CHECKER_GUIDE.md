# Maker-Checker Implementation Guide

## Overview
The Admin Dashboard now includes a comprehensive Maker-Checker workflow system that ensures dual authorization for critical operations. This prevents unauthorized changes and maintains data integrity through a two-person rule.

## User Roles

### 1. **Admin** (Full Access)
- Username: `admin`
- Password: `Admin@123`
- Can perform all operations directly without approval
- Can approve/reject pending requests

### 2. **Maker** (Create/Request Role)
- Username: `maker`
- Password: `Maker@123`
- Can create, update, and delete users
- All operations go to pending approval queue
- **CANNOT** approve their own requests

### 3. **Checker** (Approval Role)
- Username: `checker`
- Password: `Checker@123`
- Can approve or reject pending requests
- **CANNOT** approve requests they created themselves
- Can view approved and rejected history

## Key Features

### 1. **Maker-Checker Rule Enforcement**
- ✅ Maker cannot approve their own requests
- ✅ Checker cannot be the same person as Maker
- ✅ System validates this at database and application level
- ✅ Clear error messages when rules are violated

### 2. **Pending Records Management**
- All Maker operations create pending records
- Records include:
  - Operation type (Create, Update, Delete)
  - Record data (JSON format)
  - Maker information
  - Timestamp
  - Status (Pending, Approved, Rejected)

### 3. **Approval Workflow**
1. Maker creates/modifies a record → Pending state
2. Checker reviews the request → Can approve or reject
3. On approval → Operation is executed automatically
4. On rejection → Request is marked as rejected with comments

### 4. **Three Status Views**
- **Pending List**: Requests awaiting approval
- **Approved List**: Successfully approved and executed requests
- **Rejected List**: Requests that were denied with reasons

## Database Schema

### PendingRecords Table
```sql
- PendingId (INT, Primary Key, Identity)
- RecordType (NVARCHAR) - 'User', 'Role', etc.
- Operation (NVARCHAR) - 'Create', 'Update', 'Delete'
- RecordId (INT) - Original record ID for Update/Delete
- RecordData (NVARCHAR(MAX)) - JSON data of the record
- MakerId (INT) - ID of the user who created the request
- MakerName (NVARCHAR) - Name of the Maker
- CreatedDate (DATETIME) - When request was created
- Status (NVARCHAR) - 'Pending', 'Approved', 'Rejected'
- CheckerId (INT) - ID of the approver/rejecter
- CheckerName (NVARCHAR) - Name of the Checker
- CheckerComments (NVARCHAR) - Approval/rejection comments
- AuthorizedDate (DATETIME) - When approved/rejected
```

## Stored Procedures

### 1. SP_CreatePendingRecord
Creates a new pending record for approval.

### 2. SP_GetAllPendingRecords
Retrieves all pending records with optional status filter.

### 3. SP_GetPendingRecordById
Gets details of a specific pending record.

### 4. SP_ApprovePendingRecord
Approves a pending record with maker-checker validation.

### 5. SP_RejectPendingRecord
Rejects a pending record with mandatory comments.

### 6. SP_ExecuteApprovedUserOperation
Executes the approved operation (Create/Update/Delete user).

## How to Use

### As a Maker:
1. Login with maker credentials (`maker` / `Maker@123`)
2. Navigate to Users menu
3. Create, update, or delete a user
4. System shows: "User creation request submitted for approval"
5. View your pending requests in "Pending Approvals" menu

### As a Checker:
1. Login with checker credentials (`checker` / `Checker@123`)
2. Navigate to "Pending Approvals" menu
3. Click "View" on any pending request
4. Review the details and record data
5. Click "Approve" (with optional comments) or "Reject" (comments required)
6. System validates and executes the operation if approved

### As an Admin:
1. Login with admin credentials (`admin` / `Admin@123`)
2. Can perform operations directly OR
3. Can review and approve/reject pending requests
4. Has full access to all functionality

## Navigation Menu

The sidebar now includes:
- **Dashboard**: Main overview
- **Users**: User management
- **MAKER-CHECKER Section**:
  - Pending Approvals (with clock icon)
  - Approved (with checkmark icon)
  - Rejected (with X icon)

## Security Features

### 1. Authentication
- JWT token-based authentication
- HttpOnly cookies for security
- Token expiration handling

### 2. Authorization
- Role-based access control
- Claims-based user identification
- Controller-level authorization attributes

### 3. Validation
- Database-level Maker-Checker validation
- Application-level validation
- UI-level role checks

## Testing the Maker-Checker Flow

### Test Scenario 1: Normal Flow
1. Login as `maker`
2. Create a new user
3. Logout
4. Login as `checker`
5. Go to Pending Approvals
6. Approve the request
7. Verify user is created

### Test Scenario 2: Maker Cannot Approve Own Request
1. Login as `maker`
2. Create a new user
3. Stay logged in as maker
4. Try to view Pending Approvals
5. Try to approve your own request
6. System shows error: "Checker cannot be the same as Maker"

### Test Scenario 3: Rejection Flow
1. Login as `maker`
2. Create a user with invalid data
3. Logout
4. Login as `checker`
5. Review the pending request
6. Reject with reason: "Invalid data provided"
7. Request is marked as rejected
8. User is NOT created

### Test Scenario 4: Admin Direct Access
1. Login as `admin`
2. Create a user directly
3. User is created immediately without approval
4. No pending record is created

## API Endpoints

### MakerChecker Controller
- `GET /MakerChecker/PendingList` - List pending approvals
- `GET /MakerChecker/ApprovedList` - List approved requests
- `GET /MakerChecker/RejectedList` - List rejected requests
- `GET /MakerChecker/Details/{id}` - View request details
- `POST /MakerChecker/Approve` - Approve a request
- `POST /MakerChecker/Reject` - Reject a request

### User Controller (Modified)
- Checks user role before operations
- Routes Maker operations to pending records
- Admin operations execute immediately

## Technical Architecture

### Domain Layer
- `PendingRecord.cs` - Entity for pending records
- `IPendingRecordRepository.cs` - Repository interface

### Application Layer
- `PendingRecordService.cs` - Business logic for approval workflow
- `PendingRecordDto.cs` - Data transfer objects
- Validation rules for Maker-Checker

### Infrastructure Layer
- `PendingRecordRepository.cs` - ADO.NET implementation
- Stored procedure execution
- Database transaction management

### Web Layer
- `MakerCheckerController.cs` - HTTP endpoints
- Views for Pending/Approved/Rejected lists
- Details view for approval/rejection

## Troubleshooting

### Issue: "Checker cannot be the same as Maker"
**Solution**: Ensure you're logged in as a different user (Checker or Admin) to approve requests made by Maker.

### Issue: Pending record not found
**Solution**: Check if the record was already processed (approved/rejected).

### Issue: Operation not executing after approval
**Solution**: Check the application logs and database connection. Verify the SP_ExecuteApprovedUserOperation stored procedure.

### Issue: Cannot see pending approvals
**Solution**: Ensure you're logged in with Checker or Admin role. Makers can only create requests, not approve them.

## Future Enhancements

Potential improvements:
1. Email notifications for pending approvals
2. Audit trail for all changes
3. Bulk approval/rejection
4. Time-based auto-expiry of pending requests
5. Multi-level approval workflow
6. Dashboard widgets showing pending count
7. Export pending/approved/rejected reports

## Support

For issues or questions:
- Check application logs in the terminal
- Verify database connectivity
- Ensure stored procedures are created correctly
- Confirm user roles are assigned properly

---

**Created**: October 29, 2025
**Version**: 1.0
**Application**: Admin Dashboard with ASP.NET Core 9 MVC
