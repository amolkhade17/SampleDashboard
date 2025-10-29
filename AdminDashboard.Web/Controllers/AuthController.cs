using AdminDashboard.Application.DTOs;
using AdminDashboard.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        // If already logged in, redirect to dashboard
        if (Request.Cookies.ContainsKey("AuthToken"))
        {
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.LoginAsync(model);

        if (result.Success && result.Token != null)
        {
            // Store JWT token in cookie
            Response.Cookies.Append("AuthToken", result.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            // Store user info in session
            HttpContext.Session.SetString("UserId", result.User!.UserId.ToString());
            HttpContext.Session.SetString("Username", result.User.Username);
            HttpContext.Session.SetString("FullName", result.User.FullName);
            HttpContext.Session.SetString("Email", result.User.Email);
            HttpContext.Session.SetString("Role", result.User.RoleName);

            return RedirectToAction("Index", "Dashboard");
        }

        ModelState.AddModelError("", result.Message);
        return View(model);
    }

    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("AuthToken");
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
