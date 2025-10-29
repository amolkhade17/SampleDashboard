using AdminDashboard.Domain.Entities;

namespace AdminDashboard.Domain.Interfaces;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllAsync();
}
