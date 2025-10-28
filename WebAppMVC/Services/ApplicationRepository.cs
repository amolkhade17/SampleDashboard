using System.Data;
using Microsoft.Data.SqlClient;
using WebAppMVC.Models;
using WebAppMVC.Data;

namespace WebAppMVC.Services
{
    public interface IApplicationRepository
    {
        Task<List<Application>> GetActiveApplicationsAsync();
        Task<Application?> GetApplicationByIdAsync(int applicationId);
        Task<List<CrudOperation>> GetApplicationOperationsAsync(int applicationId);
    }

    public class ApplicationRepository : IApplicationRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public ApplicationRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<List<Application>> GetActiveApplicationsAsync()
        {
            var applications = new List<Application>();

            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand("sp_GetActiveApplications", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                applications.Add(new Application
                {
                    ApplicationId = reader.GetInt32("ApplicationId"),
                    ApplicationName = reader.GetString("ApplicationName"),
                    Description = reader.GetString("Description"),
                    IconClass = reader.GetString("IconClass"),
                    BackgroundColor = reader.GetString("BackgroundColor"),
                    RouteUrl = reader.GetString("RouteUrl"),
                    IsActive = reader.GetBoolean("IsActive"),
                    SortOrder = reader.GetInt32("SortOrder"),
                    CreatedDate = reader.GetDateTime("CreatedDate")
                });
            }

            return applications;
        }

        public async Task<Application?> GetApplicationByIdAsync(int applicationId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(@"
                SELECT 
                    ApplicationId,
                    ApplicationName,
                    Description,
                    IconClass,
                    BackgroundColor,
                    RouteUrl,
                    IsActive,
                    SortOrder,
                    CreatedDate
                FROM Applications 
                WHERE ApplicationId = @ApplicationId", connection);

            command.Parameters.AddWithValue("@ApplicationId", applicationId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Application
                {
                    ApplicationId = reader.GetInt32("ApplicationId"),
                    ApplicationName = reader.GetString("ApplicationName"),
                    Description = reader.GetString("Description"),
                    IconClass = reader.GetString("IconClass"),
                    BackgroundColor = reader.GetString("BackgroundColor"),
                    RouteUrl = reader.GetString("RouteUrl"),
                    IsActive = reader.GetBoolean("IsActive"),
                    SortOrder = reader.GetInt32("SortOrder"),
                    CreatedDate = reader.GetDateTime("CreatedDate")
                };
            }

            return null;
        }

        public async Task<List<CrudOperation>> GetApplicationOperationsAsync(int applicationId)
        {
            var operations = new List<CrudOperation>();

            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand("sp_GetApplicationOperations", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@ApplicationId", applicationId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                operations.Add(new CrudOperation
                {
                    OperationId = reader.GetInt32("OperationId"),
                    ApplicationId = reader.GetInt32("ApplicationId"),
                    OperationName = reader.GetString("OperationName"),
                    Description = reader.GetString("Description"),
                    IconClass = reader.GetString("IconClass"),
                    BackgroundColor = reader.GetString("BackgroundColor"),
                    ActionName = reader.GetString("ActionName"),
                    ControllerName = reader.GetString("ControllerName"),
                    IsActive = reader.GetBoolean("IsActive"),
                    SortOrder = reader.GetInt32("SortOrder"),
                    CreatedDate = reader.GetDateTime("CreatedDate")
                });
            }

            return operations;
        }
    }
}