# Getting Started Guide

## Prerequisites
- .NET 9 SDK
- SQL Server 2019 or later
- Visual Studio 2022 or VS Code

## Quick Start

### 1. Database Setup
Open SQL Server Management Studio and execute `DatabaseSetup.sql`

This will create:
- Database: `AdminDashboardDB`
- Tables: `Users`, `Roles`
- Stored Procedures for CRUD operations
- Default admin user (username: `admin`, password: `Admin@123`)

### 2. Configure Connection String
Edit `AdminDashboard.Web/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=AdminDashboardDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Replace `YOUR_SERVER_NAME` with:
- `localhost` for local SQL Server
- `.\SQLEXPRESS` for SQL Server Express
- `(localdb)\MSSQLLocalDB` for LocalDB

### 3. Build Solution
```powershell
dotnet restore
dotnet build
```

### 4. Run Application
```powershell
cd AdminDashboard.Web
dotnet run
```

Navigate to: `https://localhost:5001`

### 5. Login
Use default credentials:
- **Username**: admin
- **Password**: Admin@123

## Features Overview

### Authentication
- JWT token-based authentication
- Secure password hashing
- Session management
- Remember me functionality

### Dashboard
- Statistics cards showing key metrics
- Interactive sales chart
- Recent activities table
- Quick action buttons

### User Management
- View all users
- Create new users
- Update user information
- Delete users
- Change passwords
- Role assignment

### UI/UX
- Responsive Bootstrap 5 design
- Red and white color theme
- Sidebar navigation
- Modern card-based layout
- Font Awesome icons
- Smooth animations

## Architecture Layers

### Domain Layer
- Contains entity models (`User`, `Role`)
- Defines repository interfaces
- No dependencies on other layers

### Application Layer
- Business logic services
- DTOs (Data Transfer Objects)
- Service interfaces
- Password hashing logic

### Infrastructure Layer
- ADO.NET implementation
- Database connection factory
- Repository implementations
- JWT token service
- Stored procedure execution

### Web Layer
- MVC controllers
- Razor views
- JWT authentication middleware
- Dependency injection configuration

## Security Best Practices

1. **Change Default Password**: After first login, change the admin password
2. **Update JWT Secret**: Generate a new secret key in `appsettings.json`
3. **Use HTTPS**: Always use HTTPS in production
4. **Connection String**: Store in Azure Key Vault or user secrets for production
5. **Role-Based Access**: Implement proper role checks in controllers

## Customization

### Adding New Menu Items
Edit `Views/Shared/_DashboardLayout.cshtml` sidebar section

### Changing Theme Colors
Edit `wwwroot/css/dashboard.css`:
```css
:root {
    --primary-red: #dc3545;  /* Change this */
    --dark-red: #c82333;     /* And this */
}
```

### Adding Company Logo
1. Save your logo as `wwwroot/images/logo.png`
2. Recommended size: 200x60 pixels
3. PNG format with transparent background

### Creating New Features
1. Add stored procedure to database
2. Add method to repository interface (Domain)
3. Implement in repository (Infrastructure)
4. Create service method (Application)
5. Add controller action (Web)
6. Create view (Web)

## Troubleshooting

### Cannot Connect to Database
- Verify SQL Server is running
- Check connection string
- Ensure database exists (run DatabaseSetup.sql)
- Check firewall settings

### JWT Token Issues
- Verify JWT secret key is at least 32 characters
- Check token expiration time
- Clear browser cookies and try again

### Build Errors
```powershell
dotnet clean
dotnet restore
dotnet build
```

### Port Already in Use
Edit `Properties/launchSettings.json` to change ports

## Next Steps
- Customize the dashboard with your data
- Add more modules (Reports, Settings, etc.)
- Implement audit logging
- Add email notifications
- Deploy to Azure or IIS

## Support
For questions or issues, refer to the README.md file or contact the development team.
