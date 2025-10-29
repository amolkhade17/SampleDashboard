using System.Data.SqlClient;
using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;
using AdminDashboard.Infrastructure.Data;

namespace AdminDashboard.Infrastructure.Repositories;

/// <summary>
/// UserRepository using JSON-based configuration
/// Even cleaner - no hard-coded procedure names or parameters!
/// </summary>
public class UserRepositoryJsonBased : IUserRepository
{
    private readonly DbHelperWithConfig _dbHelper;
    private const string Entity = "User";

    public UserRepositoryJsonBased(DbHelperWithConfig dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<User?> AuthenticateAsync(string username, string passwordHash)
    {
        return await _dbHelper.ExecuteSingleAsync(
            Entity,
            "Authenticate",
            MapUserFromReader,
            new Dictionary<string, object?>
            {
                { "@Username", username },
                { "@PasswordHash", passwordHash }
            }
        );
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _dbHelper.ExecuteSingleAsync(
            Entity,
            "GetById",
            MapUserFromReader,
            new Dictionary<string, object?>
            {
                { "@UserId", userId }
            }
        );
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbHelper.ExecuteSingleAsync(
            Entity,
            "GetByUsername",
            MapUserFromReader,
            new Dictionary<string, object?>
            {
                { "@Username", username }
            }
        );
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _dbHelper.ExecuteListAsync(
            Entity,
            "GetAll",
            MapUserFromReader
        );
    }

    public async Task<int> CreateAsync(User user)
    {
        return await _dbHelper.ExecuteWithOutputAsync<int>(
            Entity,
            "Create",
            new Dictionary<string, object?>
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

    public async Task<bool> UpdateAsync(User user)
    {
        var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(
            Entity,
            "Update",
            new Dictionary<string, object?>
            {
                { "@UserId", user.UserId },
                { "@Username", user.Username },
                { "@Email", user.Email },
                { "@FullName", user.FullName },
                { "@RoleId", user.RoleId },
                { "@IsActive", user.IsActive }
            }
        );

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int userId)
    {
        var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(
            Entity,
            "Delete",
            new Dictionary<string, object?>
            {
                { "@UserId", userId }
            }
        );

        return rowsAffected > 0;
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
    {
        var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(
            Entity,
            "UpdatePassword",
            new Dictionary<string, object?>
            {
                { "@UserId", userId },
                { "@PasswordHash", newPasswordHash }
            }
        );

        return rowsAffected > 0;
    }

    public async Task<bool> UpdateStatusAsync(int userId, bool isActive)
    {
        var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(
            Entity,
            "UpdateStatus",
            new Dictionary<string, object?>
            {
                { "@UserId", userId },
                { "@IsActive", isActive }
            }
        );

        return rowsAffected > 0;
    }

    private static User MapUserFromReader(SqlDataReader reader)
    {
        return new User
        {
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            Username = reader.GetString(reader.GetOrdinal("Username")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
            RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
            ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("ModifiedDate"))
        };
    }
}
