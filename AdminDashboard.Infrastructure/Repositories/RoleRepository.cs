using System.Data;
using System.Data.SqlClient;
using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;
using AdminDashboard.Infrastructure.Data;

namespace AdminDashboard.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public RoleRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        var roles = new List<Role>();
        
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        
        command.CommandText = "SP_GetAllRoles";
        command.CommandType = CommandType.StoredProcedure;
        
        connection.Open();
        using var reader = await ((SqlCommand)command).ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            roles.Add(new Role
            {
                RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) 
                    ? null 
                    : reader.GetString(reader.GetOrdinal("Description"))
            });
        }
        
        return roles;
    }
}
