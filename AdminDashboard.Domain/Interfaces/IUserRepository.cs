using AdminDashboard.Domain.Entities;

namespace AdminDashboard.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> AuthenticateAsync(string username, string passwordHash);
    Task<User?> GetByIdAsync(int userId);
    Task<IEnumerable<User>> GetAllAsync();
    Task<int> CreateAsync(User user);
    Task<int> UpdateAsync(User user);
    Task<int> DeleteAsync(int userId);
    Task<int> ChangePasswordAsync(int userId, string newPasswordHash);
}
