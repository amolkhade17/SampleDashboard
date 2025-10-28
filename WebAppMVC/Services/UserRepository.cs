using System.Data;
using Microsoft.Data.SqlClient;
using WebAppMVC.Models;
using WebAppMVC.Data;

namespace WebAppMVC.Services
{
    public interface IUserRepository
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int id);
        Task<List<User>> GetAllUsersAsync();
        Task<User?> AuthenticateUserAsync(string username, string password);
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> UpdateLastLoginAsync(int userId);
        Task<bool> UserExistsAsync(string username, string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
    }

    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public UserRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand("sp_GetUserByUsername", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Username", username);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapUserFromReader(reader);
            }

            return null;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand("sp_GetUserByEmail", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Email", email);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapUserFromReader(reader);
            }

            return null;
        }

        public async Task<User?> AuthenticateUserAsync(string username, string password)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand("sp_AuthenticateUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Password", password);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapUserFromReader(reader);
            }

            return null;
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand("sp_CreateUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@FirstName", user.FirstName);
            command.Parameters.AddWithValue("@LastName", user.LastName);
            command.Parameters.AddWithValue("@Role", user.Role);

            await connection.OpenAsync();
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand("sp_UpdateLastLogin", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync();
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand("sp_GetUserById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserId", id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapUserFromReader(reader);
            }

            return null;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand("sp_GetAllUsers", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(MapUserFromReader(reader));
            }

            return users;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand("sp_UpdateUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserId", user.UserId);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@FirstName", user.FirstName);
            command.Parameters.AddWithValue("@LastName", user.LastName);
            command.Parameters.AddWithValue("@PhoneNumber", (object?)user.PhoneNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@IsActive", user.IsActive);
            command.Parameters.AddWithValue("@IsEmailConfirmed", user.IsEmailConfirmed);
            command.Parameters.AddWithValue("@Role", user.Role);
            command.Parameters.AddWithValue("@ProfileImageUrl", (object?)user.ProfileImageUrl ?? DBNull.Value);
            command.Parameters.AddWithValue("@Department", (object?)user.Department ?? DBNull.Value);
            command.Parameters.AddWithValue("@EmployeeId", (object?)user.EmployeeId ?? DBNull.Value);
            command.Parameters.AddWithValue("@LastModifiedBy", (object?)user.LastModifiedBy ?? DBNull.Value);

            await connection.OpenAsync();
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand("sp_DeleteUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync();
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> UserExistsAsync(string username, string email)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand("sp_CheckUserExists", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Email", email);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand("sp_CheckUsernameExists", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Username", username);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToBoolean(result);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand("sp_CheckEmailExists", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Email", email);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToBoolean(result);
        }

        private static User MapUserFromReader(SqlDataReader reader)
        {
            return new User
            {
                UserId = reader.GetInt32("UserId"),
                Username = reader.GetString("Username"),
                Email = reader.GetString("Email"),
                PasswordHash = reader.GetString("PasswordHash"),
                FirstName = reader.GetString("FirstName"),
                LastName = reader.GetString("LastName"),
                PhoneNumber = reader.IsDBNull("PhoneNumber") ? null : reader.GetString("PhoneNumber"),
                IsActive = reader.GetBoolean("IsActive"),
                IsEmailConfirmed = reader.GetBoolean("IsEmailConfirmed"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                LastLoginDate = reader.IsDBNull("LastLoginDate") ? null : reader.GetDateTime("LastLoginDate"),
                LastModifiedDate = reader.IsDBNull("LastModifiedDate") ? null : reader.GetDateTime("LastModifiedDate"),
                Role = reader.GetString("Role"),
                ProfileImageUrl = reader.IsDBNull("ProfileImageUrl") ? null : reader.GetString("ProfileImageUrl"),
                Department = reader.IsDBNull("Department") ? null : reader.GetString("Department"),
                EmployeeId = reader.IsDBNull("EmployeeId") ? null : reader.GetString("EmployeeId"),
                CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
                LastModifiedBy = reader.IsDBNull("LastModifiedBy") ? null : reader.GetInt32("LastModifiedBy")
            };
        }
    }
}