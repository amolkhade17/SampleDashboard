using System.Data;
using System.Data.SqlClient;
using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;
using AdminDashboard.Infrastructure.Data;

namespace AdminDashboard.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbHelperWithConfig _dbHelper;

    public UserRepository(DbHelperWithConfig dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<User?> AuthenticateAsync(string username, string passwordHash)
    {
        return await _dbHelper.ExecuteSingleAsync<User>(
            entity: "User",
            operation: "Authenticate",
            mapper: MapUserFromReader,
            parameterValues: new Dictionary<string, object?>
            {
                { "@Username", username },
                { "@PasswordHash", passwordHash }
            }
        );
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _dbHelper.ExecuteSingleAsync<User>(
            entity: "User",
            operation: "GetById",
            mapper: MapUserFromReader,
            parameterValues: new Dictionary<string, object?>
            {
                { "@UserId", userId }
            }
        );
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _dbHelper.ExecuteListAsync<User>(
            entity: "User",
            operation: "GetAll",
            mapper: MapUserFromReader
        );
    }

    public async Task<int> CreateAsync(User user)
    {
        return await _dbHelper.ExecuteWithOutputAsync<int>(
            entity: "User",
            operation: "Create",
            parameterValues: new Dictionary<string, object?>
            {
                { "@Username", user.Username },
                { "@PasswordHash", user.PasswordHash },
                { "@Email", user.Email },
                { "@FullName", user.FullName },
                { "@RoleId", user.RoleId },
                { "@IsActive", user.IsActive }
            }
        );
    }

    public async Task<int> UpdateAsync(User user)
    {
        return await _dbHelper.ExecuteNonQueryAsync(
            entity: "User",
            operation: "Update",
            parameterValues: new Dictionary<string, object?>
            {
                { "@UserId", user.UserId },
                { "@Username", user.Username },
                { "@Email", user.Email },
                { "@FullName", user.FullName },
                { "@RoleId", user.RoleId },
                { "@IsActive", user.IsActive }
            }
        );
    }

    public async Task<int> DeleteAsync(int userId)
    {
        return await _dbHelper.ExecuteNonQueryAsync(
            entity: "User",
            operation: "Delete",
            parameterValues: new Dictionary<string, object?>
            {
                { "@UserId", userId }
            }
        );
    }

    public async Task<int> ChangePasswordAsync(int userId, string newPasswordHash)
    {
        return await _dbHelper.ExecuteNonQueryAsync(
            entity: "User",
            operation: "UpdatePassword",
            parameterValues: new Dictionary<string, object?>
            {
                { "@UserId", userId },
                { "@PasswordHash", newPasswordHash }
            }
        );
    }

    private static User MapUserFromReader(IDataReader reader)
    {
        return new User
        {
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            Username = reader.GetString(reader.GetOrdinal("Username")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedDate = reader.IsDBNull(reader.GetOrdinal("CreatedDate")) 
                ? DateTime.MinValue 
                : reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
            LastLoginDate = reader.IsDBNull(reader.GetOrdinal("LastLoginDate")) 
                ? null 
                : reader.GetDateTime(reader.GetOrdinal("LastLoginDate")),
            RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
            RoleName = reader.GetString(reader.GetOrdinal("RoleName"))
        };
    }
}
