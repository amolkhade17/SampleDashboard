using AdminDashboard.Domain.Entities;
using System.Security.Claims;

namespace AdminDashboard.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
}
