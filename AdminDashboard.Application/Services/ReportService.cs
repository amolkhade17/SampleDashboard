using AdminDashboard.Application.DTOs;
using AdminDashboard.Domain.Interfaces;

namespace AdminDashboard.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<IEnumerable<UserActivityReportDto>> GetUserActivityReportAsync()
    {
        var result = await _reportRepository.GetUserActivityReportAsync();
        return result.Cast<UserActivityReportDto>();
    }

    public async Task<IEnumerable<ProductStockReportDto>> GetProductStockReportAsync()
    {
        var result = await _reportRepository.GetProductStockReportAsync();
        return result.Cast<ProductStockReportDto>();
    }

    public async Task<IEnumerable<MakerCheckerSummaryDto>> GetMakerCheckerSummaryAsync()
    {
        var result = await _reportRepository.GetMakerCheckerSummaryAsync();
        return result.Cast<MakerCheckerSummaryDto>();
    }
}
