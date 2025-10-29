using System.Data;
using System.Data.SqlClient;
using AdminDashboard.Application.DTOs;
using AdminDashboard.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AdminDashboard.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly string _connectionString;

    public ReportRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<IEnumerable<dynamic>> GetUserActivityReportAsync()
    {
        var reports = new List<UserActivityReportDto>();

        using (var connection = new SqlConnection(_connectionString))
        {
            using (var command = new SqlCommand("SP_GetUserActivityReport", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        reports.Add(new UserActivityReportDto
                        {
                            Username = reader.GetString(reader.GetOrdinal("Username")),
                            FullName = reader.GetString(reader.GetOrdinal("FullName")),
                            RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                            TotalRequests = reader.GetInt32(reader.GetOrdinal("TotalRequests")),
                            PendingRequests = reader.GetInt32(reader.GetOrdinal("PendingRequests")),
                            ApprovedRequests = reader.GetInt32(reader.GetOrdinal("ApprovedRequests")),
                            RejectedRequests = reader.GetInt32(reader.GetOrdinal("RejectedRequests")),
                            LastActivityDate = reader.IsDBNull(reader.GetOrdinal("LastActivityDate")) 
                                ? null 
                                : reader.GetDateTime(reader.GetOrdinal("LastActivityDate"))
                        });
                    }
                }
            }
        }

        return reports;
    }

    public async Task<IEnumerable<dynamic>> GetProductStockReportAsync()
    {
        var reports = new List<ProductStockReportDto>();

        using (var connection = new SqlConnection(_connectionString))
        {
            using (var command = new SqlCommand("SP_GetProductStockReport", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        reports.Add(new ProductStockReportDto
                        {
                            ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                            ProductCode = reader.GetString(reader.GetOrdinal("ProductCode")),
                            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                            Category = reader.GetString(reader.GetOrdinal("Category")),
                            StockQuantity = reader.GetInt32(reader.GetOrdinal("StockQuantity")),
                            UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                            TotalValue = reader.GetDecimal(reader.GetOrdinal("TotalValue")),
                            StockStatus = reader.GetString(reader.GetOrdinal("StockStatus")),
                            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                        });
                    }
                }
            }
        }

        return reports;
    }

    public async Task<IEnumerable<dynamic>> GetMakerCheckerSummaryAsync()
    {
        var reports = new List<MakerCheckerSummaryDto>();

        using (var connection = new SqlConnection(_connectionString))
        {
            using (var command = new SqlCommand("SP_GetMakerCheckerSummary", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        reports.Add(new MakerCheckerSummaryDto
                        {
                            RecordType = reader.GetString(reader.GetOrdinal("RecordType")),
                            TotalRequests = reader.GetInt32(reader.GetOrdinal("TotalRequests")),
                            PendingCount = reader.GetInt32(reader.GetOrdinal("PendingCount")),
                            ApprovedCount = reader.GetInt32(reader.GetOrdinal("ApprovedCount")),
                            RejectedCount = reader.GetInt32(reader.GetOrdinal("RejectedCount")),
                            ApprovalRate = reader.GetDecimal(reader.GetOrdinal("ApprovalRate")),
                            AvgProcessingTimeHours = reader.GetInt32(reader.GetOrdinal("AvgProcessingTimeHours"))
                        });
                    }
                }
            }
        }

        return reports;
    }
}
