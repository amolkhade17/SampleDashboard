using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard.Web.Controllers;

[Authorize]
public class DashboardController : BaseController
{
    public IActionResult Index()
    {
        // Check if user is authenticated
        if (!Request.Cookies.ContainsKey("AuthToken"))
        {
            return RedirectToAction("Login", "Auth");
        }

        return View();
    }
}
