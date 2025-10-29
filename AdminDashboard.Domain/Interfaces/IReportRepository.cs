namespace AdminDashboard.Domain.Interfaces;

public interface IReportRepository
{
    Task<IEnumerable<dynamic>> GetUserActivityReportAsync();
    Task<IEnumerable<dynamic>> GetProductStockReportAsync();
    Task<IEnumerable<dynamic>> GetMakerCheckerSummaryAsync();
}
