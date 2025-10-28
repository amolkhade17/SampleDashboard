using System.Data;
using Microsoft.Data.SqlClient;

namespace WebAppMVC.Data
{
    public interface IDbConnectionFactory
    {
        SqlConnection CreateConnection();
    }

    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}