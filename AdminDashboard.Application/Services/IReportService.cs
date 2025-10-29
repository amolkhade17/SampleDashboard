using AdminDashboard.Application.DTOs;

namespace AdminDashboard.Application.Services;

public interface IReportService
{
    Task<IEnumerable<UserActivityReportDto>> GetUserActivityReportAsync();
    Task<IEnumerable<ProductStockReportDto>> GetProductStockReportAsync();
    Task<IEnumerable<MakerCheckerSummaryDto>> GetMakerCheckerSummaryAsync();
}
