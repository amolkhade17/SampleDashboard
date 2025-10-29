using AdminDashboard.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard.Web.Controllers;

[Authorize]
public class DataTableDemoController : BaseController
{
    private readonly IProductService _productService;
    private readonly IUserService _userService;

    public DataTableDemoController(IProductService productService, IUserService userService)
    {
        _productService = productService;
        _userService = userService;
    }

    // Main Demo Page
    public IActionResult Index()
    {
        return View();
    }

    // Banking Transactions Table
    public IActionResult Transactions()
    {
        return View();
    }

    // Customer Accounts Table
    public IActionResult Accounts()
    {
        return View();
    }

    // Loan Applications Table
    public IActionResult LoanApplications()
    {
        return View();
    }

    // Transaction Monitoring Table
    public IActionResult Monitoring()
    {
        return View();
    }
}
