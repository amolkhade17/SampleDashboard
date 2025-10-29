using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AdminDashboard.Web.Controllers;

[Authorize]
public class BaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        // Set common ViewBag properties for all views
        ViewBag.FullName = HttpContext.Session.GetString("FullName") ?? "User";
        ViewBag.Username = HttpContext.Session.GetString("Username") ?? "user";
        ViewBag.Email = HttpContext.Session.GetString("Email") ?? "user@example.com";
        ViewBag.Role = HttpContext.Session.GetString("Role") ?? "User";
    }
}
