# Admin Dashboard Setup Instructions

## Step-by-Step Setup Guide

### Step 1: Database Setup (REQUIRED - Do This First!)

1. Open **SQL Server Management Studio (SSMS)**
2. Connect to your SQL Server instance
3. Open the file `DatabaseSetup.sql` from the project root
4. Execute the entire script (Press F5)
5. Verify the database was created:
   - Database: `AdminDashboardDB`
   - Tables: `Users`, `Roles`
   - Default admin user should be created

**Default Login Credentials:**
```
Username: admin
Password: Admin@123
```

### Step 2: Update Connection String

1. Open `AdminDashboard.Web\appsettings.json`
2. Update the connection string based on your SQL Server setup:

**For Local SQL Server:**
```json
"DefaultConnection": "Server=localhost;Database=AdminDashboardDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

**For SQL Server Express:**
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=AdminDashboardDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

**For LocalDB:**
```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=AdminDashboardDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

**For SQL Authentication:**
```json
"DefaultConnection": "Server=localhost;Database=AdminDashboardDB;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
```

### Step 3: Restore NuGet Packages

Open PowerShell in the project root directory and run:
```powershell
dotnet restore
```

### Step 4: Build the Solution

```powershell
dotnet build
```

If you encounter any build errors, try:
```powershell
dotnet clean
dotnet restore
dotnet build
```

### Step 5: Run the Application

Navigate to the Web project:
```powershell
cd AdminDashboard.Web
dotnet run
```

The application will start and display URLs like:
```
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
```

### Step 6: Access the Application

1. Open your web browser
2. Navigate to: `https://localhost:5001`
3. You'll see the login page
4. Enter credentials:
   - Username: `admin`
   - Password: `Admin@123`
5. Click **Login**

### Step 7: Verify Everything Works

After login, you should see:
- ✅ Red header with company name
- ✅ Sidebar navigation menu
- ✅ Dashboard with statistics cards
- ✅ Charts and activity tables
- ✅ User management section

## Troubleshooting

### Problem: Cannot connect to database

**Solution:**
1. Verify SQL Server is running:
   - Open **Services** (Win + R, type `services.msc`)
   - Look for "SQL Server" service
   - Ensure it's running
2. Test connection string using SSMS
3. Check if database exists: `SELECT * FROM sys.databases WHERE name = 'AdminDashboardDB'`

### Problem: Build errors

**Solution:**
```powershell
# Clean and rebuild
dotnet clean
rm -r **/bin
rm -r **/obj
dotnet restore
dotnet build
```

### Problem: Port already in use

**Solution:**
Edit `AdminDashboard.Web\Properties\launchSettings.json` and change ports:
```json
"applicationUrl": "https://localhost:5002;http://localhost:5003"
```

### Problem: JWT authentication not working

**Solution:**
1. Clear browser cookies
2. Verify JWT secret key in `appsettings.json` is at least 32 characters
3. Check token expiration time

### Problem: Login fails with "Invalid username or password"

**Solution:**
1. Verify database has the default admin user:
```sql
SELECT * FROM Users WHERE Username = 'admin'
```
2. If no user exists, run the INSERT statement from `DatabaseSetup.sql` again
3. Make sure you're using: `admin` / `Admin@123`

## Testing Stored Procedures

You can test stored procedures directly in SSMS:

```sql
-- Test authentication
EXEC SP_AuthenticateUser 
    @Username = 'admin', 
    @PasswordHash = '$2a$11$8K1p/Z9jVWZ6Pm9aJQqPh.rZUvLEZKvHzKvGzp8aQj0/rGZ8FdPWa'

-- Get all users
EXEC SP_GetAllUsers

-- Get all roles
EXEC SP_GetAllRoles
```

## Using Visual Studio 2022

1. Open `AdminDashboard.sln` in Visual Studio
2. Set `AdminDashboard.Web` as startup project (right-click → Set as Startup Project)
3. Update connection string in `appsettings.json`
4. Press F5 to run

## Production Deployment Checklist

Before deploying to production:

- [ ] Change default admin password
- [ ] Update JWT secret key with a strong random string
- [ ] Use HTTPS only
- [ ] Store connection string in Azure Key Vault or environment variables
- [ ] Enable detailed error logging
- [ ] Configure CORS if needed
- [ ] Set up database backups
- [ ] Enable IP restrictions
- [ ] Review and update authentication settings
- [ ] Test all functionality
- [ ] Replace placeholder logo with company logo

## Customization Quick Start

### Change Colors
Edit `wwwroot\css\dashboard.css`:
```css
:root {
    --primary-red: #YOUR_COLOR;
    --dark-red: #YOUR_DARK_COLOR;
}
```

### Add Company Logo
1. Save logo as `wwwroot\images\logo.png`
2. Recommended size: 200x60 pixels
3. PNG format with transparent background

### Add Menu Items
Edit `Views\Shared\_DashboardLayout.cshtml` - Find the sidebar section and add:
```html
<li class="nav-item">
    <a class="nav-link" asp-controller="YourController" asp-action="Index">
        <i class="fas fa-your-icon me-2"></i> Your Menu Item
    </a>
</li>
```

## Project Structure Overview

```
AdminDashboard/
├── DatabaseSetup.sql              ← Run this first!
├── AdminDashboard.sln             ← Open in Visual Studio
├── AdminDashboard.Domain/         ← Entities & Interfaces
├── AdminDashboard.Application/    ← Business Logic
├── AdminDashboard.Infrastructure/ ← Data Access (ADO.NET)
└── AdminDashboard.Web/            ← MVC Web App
    ├── Controllers/               ← API endpoints
    ├── Views/                     ← UI pages
    ├── wwwroot/                   ← Static files (CSS, JS)
    └── appsettings.json          ← Configuration (update this!)
```

## Need Help?

1. Check `README.md` for detailed documentation
2. Review `GETTING_STARTED.md` for feature guides
3. Examine the code comments
4. Test stored procedures in SSMS

## Success Indicators

You'll know everything is working when:
- ✅ Application runs without errors
- ✅ Login page appears with styled red theme
- ✅ You can login with admin credentials
- ✅ Dashboard displays with statistics
- ✅ Sidebar navigation works
- ✅ User management page shows admin user
- ✅ Charts render correctly
- ✅ No console errors in browser

## Quick Command Reference

```powershell
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run application
cd AdminDashboard.Web
dotnet run

# Watch mode (auto-reload on changes)
dotnet watch run

# Clean build artifacts
dotnet clean
```

---

**You're all set!** 🎉 

The application is ready to use. Login with `admin/Admin@123` and start exploring the dashboard!
