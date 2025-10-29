using System.Data.SqlClient;
using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;
using AdminDashboard.Infrastructure.Data;

namespace AdminDashboard.Infrastructure.Repositories;

/// <summary>
/// Refactored UserRepository using generic DbHelper
/// This eliminates repetitive ADO.NET code
/// </summary>
public class UserRepositoryRefactored : IUserRepository
{
    private readonly DbHelper _dbHelper;

    public UserRepositoryRefactored(DbHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<User?> AuthenticateAsync(string username, string passwordHash)
    {
        return await _dbHelper.ExecuteStoredProcedureSingleAsync(
            StoredProcedureNames.User.Authenticate,
            MapUserFromReader,
            DbHelper.CreateParameter("@Username", username),
            DbHelper.CreateParameter("@PasswordHash", passwordHash)
        );
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _dbHelper.ExecuteStoredProcedureSingleAsync(
            StoredProcedureNames.User.GetById,
            MapUserFromReader,
            DbHelper.CreateParameter("@UserId", userId)
        );
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbHelper.ExecuteStoredProcedureSingleAsync(
            StoredProcedureNames.User.GetByUsername,
            MapUserFromReader,
            DbHelper.CreateParameter("@Username", username)
        );
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _dbHelper.ExecuteStoredProcedureListAsync(
            StoredProcedureNames.User.GetAll,
            MapUserFromReader
        );
    }

    public async Task<int> CreateAsync(User user)
    {
        return await _dbHelper.ExecuteStoredProcedureWithOutputAsync<int>(
            StoredProcedureNames.User.Create,
            "@UserId",
            System.Data.SqlDbType.Int,
            DbHelper.CreateParameter("@Username", user.Username),
            DbHelper.CreateParameter("@PasswordHash", user.PasswordHash),
            DbHelper.CreateParameter("@Email", user.Email),
            DbHelper.CreateParameter("@FullName", user.FullName),
            DbHelper.CreateParameter("@RoleId", user.RoleId),
            DbHelper.CreateParameter("@IsActive", user.IsActive)
        );
    }

    public async Task<bool> UpdateAsync(User user)
    {
        var rowsAffected = await _dbHelper.ExecuteStoredProcedureNonQueryAsync(
            StoredProcedureNames.User.Update,
            DbHelper.CreateParameter("@UserId", user.UserId),
            DbHelper.CreateParameter("@Username", user.Username),
            DbHelper.CreateParameter("@Email", user.Email),
            DbHelper.CreateParameter("@FullName", user.FullName),
            DbHelper.CreateParameter("@RoleId", user.RoleId),
            DbHelper.CreateParameter("@IsActive", user.IsActive)
        );

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int userId)
    {
        var rowsAffected = await _dbHelper.ExecuteStoredProcedureNonQueryAsync(
            StoredProcedureNames.User.Delete,
            DbHelper.CreateParameter("@UserId", userId)
        );

        return rowsAffected > 0;
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
    {
        var rowsAffected = await _dbHelper.ExecuteStoredProcedureNonQueryAsync(
            "SP_UpdateUserPassword",
            DbHelper.CreateParameter("@UserId", userId),
            DbHelper.CreateParameter("@PasswordHash", newPasswordHash)
        );

        return rowsAffected > 0;
    }

    public async Task<bool> UpdateStatusAsync(int userId, bool isActive)
    {
        var rowsAffected = await _dbHelper.ExecuteStoredProcedureNonQueryAsync(
            StoredProcedureNames.User.UpdateStatus,
            DbHelper.CreateParameter("@UserId", userId),
            DbHelper.CreateParameter("@IsActive", isActive)
        );

        return rowsAffected > 0;
    }

    // Single mapper method - reusable across all methods
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
