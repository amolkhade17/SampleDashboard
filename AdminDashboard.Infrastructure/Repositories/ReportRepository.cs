using System.Data;
using System.Data.SqlClient;
using AdminDashboard.Application.DTOs;
using AdminDashboard.Domain.Interfaces;
using AdminDashboard.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace AdminDashboard.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly DbHelperWithConfig _dbHelper;

    public ReportRepository(DbHelperWithConfig dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<IEnumerable<dynamic>> GetUserActivityReportAsync()
    {
        return await _dbHelper.ExecuteListAsync<UserActivityReportDto>(
            entity: "Report",
            operation: "GetUserActivity",
            mapper: reader => new UserActivityReportDto
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
            },
            parameterValues: null
        );
    }

    public async Task<IEnumerable<dynamic>> GetProductStockReportAsync()
    {
        // Using GetProductSales as proxy for stock report
        return await _dbHelper.ExecuteListAsync<ProductStockReportDto>(
            entity: "Report",
            operation: "GetProductSales",
            mapper: reader => new ProductStockReportDto
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
            },
            parameterValues: null
        );
    }

    public async Task<IEnumerable<dynamic>> GetMakerCheckerSummaryAsync()
    {
        var result = await _dbHelper.ExecuteSingleAsync<MakerCheckerSummaryDto>(
            entity: "Report",
            operation: "GetMakerCheckerSummary",
            mapper: reader => new MakerCheckerSummaryDto
            {
                RecordType = reader.GetString(reader.GetOrdinal("RecordType")),
                TotalRequests = reader.GetInt32(reader.GetOrdinal("TotalRequests")),
                PendingCount = reader.GetInt32(reader.GetOrdinal("PendingCount")),
                ApprovedCount = reader.GetInt32(reader.GetOrdinal("ApprovedCount")),
                RejectedCount = reader.GetInt32(reader.GetOrdinal("RejectedCount")),
                ApprovalRate = reader.GetDecimal(reader.GetOrdinal("ApprovalRate")),
                AvgProcessingTimeHours = reader.GetInt32(reader.GetOrdinal("AvgProcessingTimeHours"))
            },
            parameterValues: null
        );

        return result != null ? new[] { result } : Array.Empty<MakerCheckerSummaryDto>();
    }
}
