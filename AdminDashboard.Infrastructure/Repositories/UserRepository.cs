using System.Data;
using System.Data.SqlClient;
using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;
using AdminDashboard.Infrastructure.Data;

namespace AdminDashboard.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public UserRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> AuthenticateAsync(string username, string passwordHash)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        
        command.CommandText = "SP_AuthenticateUser";
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.Add(new SqlParameter("@Username", username));
        command.Parameters.Add(new SqlParameter("@PasswordHash", passwordHash));
        
        connection.Open();
        using var reader = await ((SqlCommand)command).ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return MapUserFromReader(reader);
        }
        
        return null;
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        
        command.CommandText = "SP_GetUserById";
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.Add(new SqlParameter("@UserId", userId));
        
        connection.Open();
        using var reader = await ((SqlCommand)command).ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return MapUserFromReader(reader);
        }
        
        return null;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        var users = new List<User>();
        
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        
        command.CommandText = "SP_GetAllUsers";
        command.CommandType = CommandType.StoredProcedure;
        
        connection.Open();
        using var reader = await ((SqlCommand)command).ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            users.Add(MapUserFromReader(reader));
        }
        
        return users;
    }

    public async Task<int> CreateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        
        command.CommandText = "SP_CreateUser";
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.Add(new SqlParameter("@Username", user.Username));
        command.Parameters.Add(new SqlParameter("@PasswordHash", user.PasswordHash));
        command.Parameters.Add(new SqlParameter("@Email", user.Email));
        command.Parameters.Add(new SqlParameter("@FullName", user.FullName));
        command.Parameters.Add(new SqlParameter("@RoleId", user.RoleId));
        command.Parameters.Add(new SqlParameter("@IsActive", user.IsActive));
        
        connection.Open();
        var result = await ((SqlCommand)command).ExecuteScalarAsync();
        
        return Convert.ToInt32(result);
    }

    public async Task<int> UpdateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        
        command.CommandText = "SP_UpdateUser";
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.Add(new SqlParameter("@UserId", user.UserId));
        command.Parameters.Add(new SqlParameter("@Username", user.Username));
        command.Parameters.Add(new SqlParameter("@Email", user.Email));
        command.Parameters.Add(new SqlParameter("@FullName", user.FullName));
        command.Parameters.Add(new SqlParameter("@RoleId", user.RoleId));
        command.Parameters.Add(new SqlParameter("@IsActive", user.IsActive));
        
        connection.Open();
        var result = await ((SqlCommand)command).ExecuteScalarAsync();
        
        return Convert.ToInt32(result);
    }

    public async Task<int> DeleteAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        
        command.CommandText = "SP_DeleteUser";
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.Add(new SqlParameter("@UserId", userId));
        
        connection.Open();
        var result = await ((SqlCommand)command).ExecuteScalarAsync();
        
        return Convert.ToInt32(result);
    }

    public async Task<int> ChangePasswordAsync(int userId, string newPasswordHash)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        
        command.CommandText = "SP_ChangePassword";
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.Add(new SqlParameter("@UserId", userId));
        command.Parameters.Add(new SqlParameter("@NewPasswordHash", newPasswordHash));
        
        connection.Open();
        var result = await ((SqlCommand)command).ExecuteScalarAsync();
        
        return Convert.ToInt32(result);
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
