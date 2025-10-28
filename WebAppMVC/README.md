# WebApp MVC - ASP.NET Core 9 Web Application

A modern, Material Design-inspired web application built with ASP.NET Core 9 MVC, featuring JWT authentication, SQL Server integration with stored procedures, and a rich Bootstrap 5 UI.

## Features

- **JWT Authentication**: Secure token-based authentication system
- **Material Design UI**: Rich, interactive interface using Bootstrap 5 with Material Design principles
- **SQL Server Integration**: Direct database operations using stored procedures (no Entity Framework)
- **Modular Architecture**: Clean separation of concerns with services and repositories
- **Responsive Design**: Mobile-friendly interface with modern animations and transitions
- **Application Dashboard**: Card-based navigation system for different application modules
- **CRUD Operations**: Demonstration of Create, Read, Update, Delete operations for different entities

## Technology Stack

- **Backend**: ASP.NET Core 9 MVC
- **Authentication**: JWT (JSON Web Tokens)
- **Database**: Microsoft SQL Server
- **Frontend**: Bootstrap 5, Font Awesome, Google Fonts
- **Styling**: Material Design principles with custom CSS
- **JavaScript**: jQuery for enhanced interactions

## Project Structure

```
WebAppMVC/
├── Controllers/          # MVC Controllers
├── Models/              # Data models
├── ViewModels/          # View-specific models
├── Services/            # Business logic and authentication services
├── Data/                # Database connection and data access
├── Views/               # Razor views and layouts
├── Database/            # SQL scripts for schema and stored procedures
└── wwwroot/            # Static files (CSS, JS, images)
```

## Database Setup

1. **Create Database**: Run the SQL scripts in the `Database` folder in the following order:
   ```sql
   -- 1. Create the database schema and sample data
   Database/01_CreateSchema.sql
   
   -- 2. Create user authentication stored procedures
   Database/02_UserStoredProcedures.sql
   
   -- 3. Create application and CRUD operation stored procedures
   Database/03_ApplicationStoredProcedures.sql
   ```

2. **Update Connection String**: Modify the connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WebAppMVCDb;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

## Getting Started

1. **Clone/Download** the project files

2. **Setup Database**: 
   - Execute the SQL scripts in the `Database` folder
   - Update the connection string in `appsettings.json`

3. **Run the Application**:
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

4. **Access the Application**:
   - Navigate to `https://localhost:5001` or `http://localhost:5000`
   - Use the demo credentials provided on the login page

## Demo Credentials

The application comes with pre-configured demo users:

- **Admin User**: 
  - Username: `admin`
  - Password: `admin123`
  - Role: Admin

- **Regular User**:
  - Username: `user`
  - Password: `user123`
  - Role: User

## Key Features Explained

### 1. Authentication System
- JWT token-based authentication
- Session management for token storage
- Role-based access control
- Secure password hashing using HMACSHA512

### 2. Material Design UI
- Modern card-based layouts
- Smooth animations and transitions
- Gradient backgrounds and shadows
- Interactive form elements with floating labels
- Responsive design for all screen sizes

### 3. Application Dashboard
- Card-style navigation tiles for different applications
- Color-coded applications with custom icons
- Hover effects and smooth animations
- Breadcrumb navigation

### 4. CRUD Operations Interface
- Dedicated pages for Create, Read, Update, Delete operations
- Consistent styling across all operation types
- Demo pages showing the structure for real implementations
- Integration points for stored procedure calls

### 5. Database Integration
- Direct SQL Server connectivity without Entity Framework
- Stored procedures for all database operations
- Parameterized queries for security
- Transaction support for data consistency

## Customization

### Adding New Applications
1. Insert new records into the `Applications` table
2. Add corresponding CRUD operations in the `CrudOperations` table
3. Create controllers and views for the new application
4. Update navigation as needed

### Styling Customization
- Modify CSS variables in the layout files for theme changes
- Update Bootstrap classes for different visual styles
- Add custom animations by extending the existing CSS

### Authentication Customization
- Modify JWT settings in `appsettings.json`
- Update password hashing algorithm in `AuthenticationService`
- Add additional user properties in the `User` model

## Security Considerations

- JWT tokens are stored in session (server-side)
- Passwords are hashed using HMACSHA512 with salt
- SQL injection prevention through parameterized queries
- HTTPS redirection enabled by default
- Anti-forgery token validation on forms

## Performance Features

- Async/await pattern throughout the application
- Efficient database connection management
- Optimized CSS and JavaScript loading
- Responsive images and lazy loading support

## Browser Support

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This project is provided as-is for educational and demonstration purposes.

## Support

For issues or questions, please review the code comments and documentation. The application is designed to be self-explanatory with extensive inline documentation.