# Deployment Guide

## ⚠️ Troubleshooting: "Login Error:hashpassword" Issue

**If you see "Login Error:hashpassword" when deploying to a server or another system**, this typically indicates one of the following issues:

### Quick Fix Steps:

1. **Verify IPasswordHasher Service Registration**
   - Ensure `AdminDashboard.Application.dll` is deployed
   - Check `Program.cs` calls `builder.Services.AddApplication()`

2. **Check Password Hash Format in Database**
   ```sql
   SELECT Username, LEN(PasswordHash) AS HashLength FROM Users
   ```
   - Expected: HashLength should be **64** characters
   - If different, passwords need to be regenerated

3. **Regenerate Admin Password Hash**
   
   Run this in the `HashGenerator` project or any C# console:
   ```csharp
   using System.Security.Cryptography;
   
   string password = "Admin@123";
   const int SaltSize = 16, HashSize = 32, Iterations = 10000;
   
   using var rng = RandomNumberGenerator.Create();
   var salt = new byte[SaltSize];
   rng.GetBytes(salt);
   
   using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
   var hash = pbkdf2.GetBytes(HashSize);
   
   var hashBytes = new byte[SaltSize + HashSize];
   Array.Copy(salt, 0, hashBytes, 0, SaltSize);
   Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);
   
   Console.WriteLine(Convert.ToBase64String(hashBytes));
   ```
   
   Then update database:
   ```sql
   UPDATE Users 
   SET PasswordHash = 'YOUR_GENERATED_64_CHAR_HASH'
   WHERE Username = 'admin'
   ```

4. **Verify appsettings.json is Deployed**
   - Connection string must point to correct SQL Server
   - JWT section must be complete with SecretKey, Issuer, Audience

5. **Check DLL Files are Present**
   Required files in deployment folder:
   - `AdminDashboard.Web.dll`
   - `AdminDashboard.Application.dll` ← **Critical for password hashing**
   - `AdminDashboard.Infrastructure.dll`
   - `AdminDashboard.Domain.dll`

6. **Verify Database Connectivity**
   ```sql
   -- Test from server
   sqlcmd -S YOUR_SERVER -d AdminDashboardDB -Q "SELECT COUNT(*) FROM Users"
   ```

7. **Enable Detailed Errors (Temporarily)**
   In `Program.cs`, add before `var app = builder.Build();`:
   ```csharp
   builder.Services.AddDatabaseDeveloperPageExceptionFilter();
   ```
   
   And after `var app = builder.Build();`:
   ```csharp
   app.UseDeveloperExceptionPage(); // Shows full error details
   ```
   **⚠️ Remove these in production!**

### Root Causes:

- **Missing AdminDashboard.Application.dll** - IPasswordHasher not available
- **Wrong password format in database** - Hash not 64 characters (Base64 encoded 48 bytes)
- **Database connection failure** - Can't retrieve user data
- **Dependency injection not configured** - AddApplication() not called

---

## Pre-Deployment Checklist

Before deploying to production, ensure:

- [ ] All tests pass
- [ ] Database is backed up
- [ ] Connection strings are secured
- [ ] JWT secret key is changed
- [ ] Default admin password is changed
- [ ] HTTPS is enabled
- [ ] Error logging is configured
- [ ] Application is built in Release mode

## Deployment Options

### Option 1: IIS Deployment (Windows Server)

#### Prerequisites
- Windows Server 2019 or later
- IIS 10 or later
- .NET 9 Hosting Bundle
- SQL Server

#### Steps

1. **Install .NET 9 Hosting Bundle**
   - Download from: https://dotnet.microsoft.com/download/dotnet/9.0
   - Run the installer
   - Restart IIS: `iisreset`

2. **Prepare Application**
   ```powershell
   cd AdminDashboard.Web
   dotnet publish -c Release -o C:\Publish\AdminDashboard
   ```

3. **Setup Database**
   - Execute `DatabaseSetup.sql` on production SQL Server
   - Update connection string in `appsettings.json`

4. **Configure IIS**
   - Open IIS Manager
   - Create new Application Pool:
     - Name: AdminDashboard
     - .NET CLR Version: No Managed Code
     - Managed Pipeline Mode: Integrated
   - Create new Website:
     - Site name: AdminDashboard
     - Application Pool: AdminDashboard
     - Physical path: C:\Publish\AdminDashboard
     - Port: 80 (HTTP) or 443 (HTTPS)
   - Configure SSL certificate if using HTTPS

5. **Set Permissions**
   ```powershell
   icacls "C:\Publish\AdminDashboard" /grant "IIS AppPool\AdminDashboard:(OI)(CI)F" /T
   ```

6. **Configure Web.config** (auto-generated)
   Verify the web.config has:
   ```xml
   <aspNetCore processPath="dotnet" 
               arguments=".\AdminDashboard.Web.dll" 
               stdoutLogEnabled="true" 
               stdoutLogFile=".\logs\stdout" />
   ```

7. **Test Deployment**
   - Browse to: http://yourserver or https://yourserver
   - Login with admin credentials
   - Verify all features work

### Option 2: Azure App Service Deployment

#### Prerequisites
- Azure Subscription
- Azure SQL Database

#### Steps

1. **Create Azure Resources**
   ```powershell
   # Login to Azure
   az login
   
   # Create Resource Group
   az group create --name AdminDashboard-RG --location eastus
   
   # Create App Service Plan
   az appservice plan create --name AdminDashboard-Plan --resource-group AdminDashboard-RG --sku B1 --is-linux
   
   # Create Web App
   az webapp create --name YourAppName --resource-group AdminDashboard-RG --plan AdminDashboard-Plan --runtime "DOTNET:9.0"
   
   # Create Azure SQL Server
   az sql server create --name yoursqlserver --resource-group AdminDashboard-RG --location eastus --admin-user sqladmin --admin-password YourPassword123!
   
   # Create Database
   az sql db create --resource-group AdminDashboard-RG --server yoursqlserver --name AdminDashboardDB --service-objective S0
   ```

2. **Configure Connection String**
   ```powershell
   az webapp config connection-string set --name YourAppName --resource-group AdminDashboard-RG --connection-string-type SQLAzure --settings DefaultConnection="Server=tcp:yoursqlserver.database.windows.net,1433;Database=AdminDashboardDB;User ID=sqladmin;Password=YourPassword123!;Encrypt=True;TrustServerCertificate=False;"
   ```

3. **Configure App Settings**
   ```powershell
   az webapp config appsettings set --name YourAppName --resource-group AdminDashboard-RG --settings Jwt__SecretKey="YourProductionSecretKeyHere" Jwt__Issuer="AdminDashboard" Jwt__Audience="AdminDashboard" Jwt__ExpirationMinutes="60"
   ```

4. **Deploy Application**
   ```powershell
   # Publish
   cd AdminDashboard.Web
   dotnet publish -c Release -o ./publish
   
   # Deploy (using ZIP)
   Compress-Archive -Path ./publish/* -DestinationPath ./publish.zip -Force
   az webapp deploy --name YourAppName --resource-group AdminDashboard-RG --src-path ./publish.zip --type zip
   ```

5. **Setup Database**
   - Connect to Azure SQL using SSMS
   - Execute `DatabaseSetup.sql`

6. **Configure HTTPS**
   - Azure App Service provides HTTPS by default
   - Optional: Add custom domain and SSL certificate

### Option 3: Docker Deployment

#### Prerequisites
- Docker installed
- Docker Hub account (optional)

#### Steps

1. **Create Dockerfile**
   Create `Dockerfile` in project root:
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
   WORKDIR /app
   EXPOSE 80
   EXPOSE 443

   FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
   WORKDIR /src
   COPY ["AdminDashboard.Web/AdminDashboard.Web.csproj", "AdminDashboard.Web/"]
   COPY ["AdminDashboard.Application/AdminDashboard.Application.csproj", "AdminDashboard.Application/"]
   COPY ["AdminDashboard.Domain/AdminDashboard.Domain.csproj", "AdminDashboard.Domain/"]
   COPY ["AdminDashboard.Infrastructure/AdminDashboard.Infrastructure.csproj", "AdminDashboard.Infrastructure/"]
   RUN dotnet restore "AdminDashboard.Web/AdminDashboard.Web.csproj"
   COPY . .
   WORKDIR "/src/AdminDashboard.Web"
   RUN dotnet build "AdminDashboard.Web.csproj" -c Release -o /app/build

   FROM build AS publish
   RUN dotnet publish "AdminDashboard.Web.csproj" -c Release -o /app/publish

   FROM base AS final
   WORKDIR /app
   COPY --from=publish /app/publish .
   ENTRYPOINT ["dotnet", "AdminDashboard.Web.dll"]
   ```

2. **Create docker-compose.yml**
   ```yaml
   version: '3.8'
   services:
     web:
       build: .
       ports:
         - "5000:80"
         - "5001:443"
       environment:
         - ASPNETCORE_ENVIRONMENT=Production
         - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=AdminDashboardDB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;
       depends_on:
         - sqlserver
     
     sqlserver:
       image: mcr.microsoft.com/mssql/server:2022-latest
       environment:
         - ACCEPT_EULA=Y
         - SA_PASSWORD=YourPassword123!
       ports:
         - "1433:1433"
       volumes:
         - sqldata:/var/opt/mssql
   
   volumes:
     sqldata:
   ```

3. **Build and Run**
   ```powershell
   docker-compose build
   docker-compose up -d
   ```

4. **Setup Database**
   ```powershell
   # Wait for SQL Server to start (30 seconds)
   Start-Sleep -Seconds 30
   
   # Execute setup script
   docker exec -it admindashboard_sqlserver_1 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourPassword123! -i /DatabaseSetup.sql
   ```

## Production Configuration

### appsettings.Production.json

Create this file in `AdminDashboard.Web/`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "yourdomain.com,www.yourdomain.com"
}
```

### Secure Configuration

**Use Azure Key Vault or Environment Variables for:**
- Connection strings
- JWT secret key
- API keys
- Passwords

**Example using Environment Variables:**
```powershell
# Set environment variable
$env:ConnectionStrings__DefaultConnection="Server=prod-server;Database=AdminDashboardDB;..."
$env:Jwt__SecretKey="YourProductionSecretKey"
```

### Enable HTTPS

**Update Program.cs:**
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
```

**Configure HTTPS in IIS:**
1. Obtain SSL certificate
2. Bind certificate to website in IIS
3. Enforce HTTPS

## Post-Deployment Tasks

### 1. Change Default Password
```sql
-- Update admin password immediately
EXEC SP_ChangePassword @UserId = 1, @NewPasswordHash = 'new-hashed-password'
```

### 2. Configure Logging
Install logging provider (e.g., Serilog):
```powershell
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
```

### 3. Setup Monitoring
- Enable Application Insights (Azure)
- Configure health checks
- Setup uptime monitoring

### 4. Database Backup
```sql
-- Schedule automated backups
BACKUP DATABASE AdminDashboardDB 
TO DISK = 'C:\Backups\AdminDashboardDB.bak'
WITH FORMAT, INIT, NAME = 'Full Backup';
```

### 5. Performance Optimization
- Enable response compression
- Configure caching
- Optimize database indexes
- Use CDN for static assets

## Troubleshooting

### Issue: 500 Internal Server Error

**Solution:**
1. Enable detailed errors temporarily:
   ```json
   "DetailedErrors": true,
   "Logging": { "LogLevel": { "Default": "Debug" } }
   ```
2. Check logs in IIS or Azure
3. Verify connection string
4. Check database connectivity

### Issue: Database Connection Fails

**Solution:**
1. Verify SQL Server is running
2. Check firewall rules
3. Test connection string manually
4. Verify SQL authentication mode

### Issue: JWT Token Not Working

**Solution:**
1. Verify secret key is same across deployments
2. Check token expiration
3. Clear browser cookies
4. Verify HTTPS configuration

## Security Recommendations

1. **Use Strong Passwords**
   - Minimum 12 characters
   - Include uppercase, lowercase, numbers, symbols

2. **Enable IP Restrictions**
   ```xml
   <security>
     <ipSecurity allowUnlisted="false">
       <add ipAddress="192.168.1.0" subnetMask="255.255.255.0" allowed="true" />
     </ipSecurity>
   </security>
   ```

3. **Configure CORS (if needed)**
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("ProductionPolicy",
           builder => builder.WithOrigins("https://yourdomain.com")
                            .AllowAnyMethod()
                            .AllowAnyHeader());
   });
   ```

4. **Enable Rate Limiting**
   Install AspNetCoreRateLimit package

5. **Regular Security Updates**
   - Keep .NET updated
   - Update NuGet packages
   - Monitor security advisories

## Backup Strategy

### Database Backups
```sql
-- Full backup daily
BACKUP DATABASE AdminDashboardDB TO DISK = 'C:\Backups\Full\AdminDashboardDB.bak'

-- Transaction log backup hourly
BACKUP LOG AdminDashboardDB TO DISK = 'C:\Backups\Log\AdminDashboardDB.trn'
```

### Application Backups
- Backup published files
- Backup configuration files
- Store in separate location
- Test restore procedures

## Scaling Considerations

### Horizontal Scaling
- Use Azure App Service with multiple instances
- Implement session state sharing (Redis)
- Use load balancer

### Vertical Scaling
- Increase server resources (CPU, RAM)
- Upgrade database tier
- Optimize database queries

## Maintenance

### Regular Tasks
- [ ] Check application logs weekly
- [ ] Review database performance monthly
- [ ] Update packages quarterly
- [ ] Security audit annually
- [ ] Backup verification monthly

### Updates
```powershell
# Update all packages
dotnet outdated
dotnet add package [PackageName] --version [NewVersion]
```

## Support & Monitoring

### Setup Alerts
- Database connection failures
- High CPU/Memory usage
- Failed login attempts
- Error rate threshold

### Monitoring Tools
- Application Insights (Azure)
- Azure Monitor
- ELK Stack (Elasticsearch, Logstash, Kibana)
- Prometheus + Grafana

---

**Deployment Complete!** 🚀

Remember to:
- Change default credentials
- Enable HTTPS
- Configure backups
- Setup monitoring
- Document any customizations
