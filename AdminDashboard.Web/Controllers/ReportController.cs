using AdminDashboard.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard.Web.Controllers;

[Authorize]
public class ReportController : BaseController
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // GET: Report/Index
    public IActionResult Index()
    {
        return View();
    }

    // GET: Report/UserActivity
    public async Task<IActionResult> UserActivity()
    {
        var report = await _reportService.GetUserActivityReportAsync();
        return View(report);
    }

    // GET: Report/ProductStock
    public async Task<IActionResult> ProductStock()
    {
        var report = await _reportService.GetProductStockReportAsync();
        return View(report);
    }

    // GET: Report/MakerCheckerSummary
    public async Task<IActionResult> MakerCheckerSummary()
    {
        var report = await _reportService.GetMakerCheckerSummaryAsync();
        return View(report);
    }
}
