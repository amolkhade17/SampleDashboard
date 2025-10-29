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
        services.AddSingleton<DbConnectionFactory>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPendingRecordRepository, PendingRecordRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        
        return services;
    }
}
