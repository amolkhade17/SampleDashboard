using AdminDashboard.Application.Interfaces;
using AdminDashboard.Domain.Interfaces;
using AdminDashboard.Infrastructure.Data;
using AdminDashboard.Infrastructure.Repositories;
using AdminDashboard.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AdminDashboard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Legacy approach (existing)
        services.AddSingleton<DbConnectionFactory>();
        
        // NEW: Generic Database Helper (recommended for new code)
        services.AddScoped<DbHelper>();
        
        // NEWEST: JSON-based Configuration Approach (Modular - RECOMMENDED)
        services.AddSingleton<IStoredProcedureConfigService, ModularStoredProcedureConfigService>();
        services.AddScoped<DbHelperWithConfig>();
        
        // Legacy: Single-file JSON configuration (fallback)
        // services.AddSingleton<IStoredProcedureConfigService, StoredProcedureConfigService>();
        // services.AddScoped<DbHelperWithConfig>();
        
        // Repository registrations
        // Option 1: Use existing repositories (current - ACTIVE)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPendingRecordRepository, PendingRecordRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IUploadedFileRepository, UploadedFileRepository>();
        
        // Option 2: Switch to refactored repositories with constants
        // services.AddScoped<IUserRepository, UserRepositoryRefactored>();
        // services.AddScoped<IProductRepository, ProductRepositoryRefactored>();
        // services.AddScoped<IPendingRecordRepository, MakerCheckerRepositoryRefactored>();
        
        // Option 3: Switch to JSON-based repositories (most flexible)
        // services.AddScoped<IUserRepository, UserRepositoryJsonBased>();
        
        // Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        
        return services;
    }
}
