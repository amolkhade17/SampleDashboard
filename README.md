# Admin Dashboard - ASP.NET Core 9 MVC

## Overview
Enterprise-grade Admin Dashboard built with ASP.NET Core 9 MVC following Clean Architecture principles. Features JWT authentication, ADO.NET with stored procedures, and modern Bootstrap 5 UI with red-white theme.

## Architecture
- **Clean Architecture** with 4 layers:
  - `Domain` - Entities and Interfaces
  - `Application` - Business Logic and DTOs
  - `Infrastructure` - Data Access (ADO.NET) and Services
  - `Web` - MVC UI Layer

## Technology Stack
- **Framework**: ASP.NET Core 9 MVC
- **Database**: Microsoft SQL Server
- **Data Access**: ADO.NET with Stored Procedures
- **Authentication**: JWT Token-based
- **Frontend**: Bootstrap 5, jQuery, Font Awesome
- **Design**: Red & White Theme with Modern Enterprise UI

## Features
- ✅ JWT Token Authentication
- ✅ Role-based Access Control
- ✅ User Management (CRUD)
- ✅ Responsive Dashboard with Sidebar Navigation
- ✅ Modern Bootstrap 5 UI
- ✅ Stored Procedure-based Data Operations
- ✅ Clean Architecture Pattern
- ✅ Secure Password Hashing
- ✅ Session Management

## Setup Instructions

### 1. Database Setup
Run the SQL script to create the database:
```bash
sqlcmd -S localhost -i DatabaseSetup.sql
```
Or execute `DatabaseSetup.sql` in SQL Server Management Studio.

**Default Login Credentials:**
- Username: `admin`
- Password: `Admin@123`

### 2. Connection String
Update the connection string in `AdminDashboard.Web/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=AdminDashboardDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Build & Run
```bash
cd "AdminDashboard.Web"
dotnet restore
dotnet build
dotnet run
```

The application will be available at: `https://localhost:5001` or `http://localhost:5000`

## Project Structure
```
AdminDashboard/
├── AdminDashboard.Domain/         # Domain Entities & Interfaces
│   ├── Entities/
│   │   ├── User.cs
│   │   └── Role.cs
│   └── Interfaces/
│       ├── IUserRepository.cs
│       └── IRoleRepository.cs
│
├── AdminDashboard.Application/    # Business Logic
│   ├── DTOs/
│   │   └── UserDtos.cs
│   └── Services/
│       ├── AuthService.cs
│       ├── UserService.cs
│       └── PasswordHasher.cs
│
├── AdminDashboard.Infrastructure/ # Data Access & JWT
│   ├── Data/
│   │   └── DbConnectionFactory.cs
│   ├── Repositories/
│   │   ├── UserRepository.cs
│   │   └── RoleRepository.cs
│   └── Services/
│       └── JwtTokenService.cs
│
└── AdminDashboard.Web/            # MVC Web Application
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── DashboardController.cs
    │   └── UserController.cs
    ├── Views/
    │   ├── Auth/Login.cshtml
    │   ├── Dashboard/Index.cshtml
    │   └── User/Index.cshtml
    └── wwwroot/
        ├── css/
        └── js/

DatabaseSetup.sql                  # Database & Stored Procedures
```

## Database Stored Procedures
- `SP_AuthenticateUser` - User authentication
- `SP_GetUserById` - Get user by ID
- `SP_GetAllUsers` - Get all users
- `SP_CreateUser` - Create new user
- `SP_UpdateUser` - Update user details
- `SP_DeleteUser` - Delete user
- `SP_ChangePassword` - Change user password
- `SP_GetAllRoles` - Get all roles

## Security Features
- JWT token-based authentication
- Secure password hashing (PBKDF2 with SHA256)
- HttpOnly cookies for token storage
- Role-based authorization
- Session management
- CSRF protection

## UI Features
- Modern responsive design
- Red header with company logo placeholder
- Collapsible sidebar navigation
- Dashboard with statistics cards
- Interactive charts (Chart.js)
- User management interface
- Alert notifications
- Mobile-friendly layout

## Customization
- Update company logo: Replace `wwwroot/images/logo.png`
- Modify colors: Edit CSS variables in `wwwroot/css/dashboard.css`
- Add menu items: Update sidebar in `Views/Shared/_DashboardLayout.cshtml`
- Configure JWT settings: Modify `appsettings.json` Jwt section

## Development
To add new features:
1. Create entities in `Domain` layer
2. Add stored procedures in database
3. Implement repositories in `Infrastructure`
4. Create services in `Application`
5. Build controllers and views in `Web`

## License
This is a proprietary enterprise application.

## Support
For issues or questions, contact the development team.
