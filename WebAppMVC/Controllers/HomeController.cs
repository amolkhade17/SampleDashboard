using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebAppMVC.Models;
using WebAppMVC.Services;

namespace WebAppMVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IApplicationRepository _applicationRepository;

    public HomeController(ILogger<HomeController> logger, IApplicationRepository applicationRepository)
    {
        _logger = logger;
        _applicationRepository = applicationRepository;
    }

    public IActionResult Index()
    {
        // If user is authenticated, redirect to dashboard
        if (IsUserAuthenticated())
        {
            return RedirectToAction("Dashboard");
        }
        
        // Otherwise redirect to login
        return RedirectToAction("Login", "Account");
    }

    public async Task<IActionResult> Dashboard()
    {
        if (!IsUserAuthenticated())
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var applications = await _applicationRepository.GetActiveApplicationsAsync();
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.Role = HttpContext.Session.GetString("Role");
            return View(applications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            TempData["ErrorMessage"] = "Error loading dashboard. Please try again.";
            return View(new List<Application>());
        }
    }

    public async Task<IActionResult> ApplicationOperations(int id)
    {
        if (!IsUserAuthenticated())
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var application = await _applicationRepository.GetApplicationByIdAsync(id);
            if (application == null)
            {
                TempData["ErrorMessage"] = "Application not found.";
                return RedirectToAction("Dashboard");
            }

            var operations = await _applicationRepository.GetApplicationOperationsAsync(id);
            
            ViewBag.Application = application;
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.Role = HttpContext.Session.GetString("Role");
            
            return View(operations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading application operations for ID {ApplicationId}", id);
            TempData["ErrorMessage"] = "Error loading application operations. Please try again.";
            return RedirectToAction("Dashboard");
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private bool IsUserAuthenticated()
    {
        var token = HttpContext.Session.GetString("JWTToken");
        return !string.IsNullOrEmpty(token);
    }
}
