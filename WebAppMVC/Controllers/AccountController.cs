using Microsoft.AspNetCore.Mvc;
using WebAppMVC.Services;
using WebAppMVC.ViewModels;
using WebAppMVC.Models;

namespace WebAppMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthenticationService authenticationService, ILogger<AccountController> logger)
        {
            _authenticationService = authenticationService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (IsUserAuthenticated())
            {
                return RedirectToAction("Dashboard", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authenticationService.LoginAsync(model.Username, model.Password);

                if (result.Success)
                {
                    // Store JWT token in session
                    HttpContext.Session.SetString("JWTToken", result.Token!);
                    HttpContext.Session.SetString("UserId", result.User!.UserId.ToString());
                    HttpContext.Session.SetString("Username", result.User.Username);
                    HttpContext.Session.SetString("Role", result.User.Role);

                    _logger.LogInformation("User {Username} logged in successfully", model.Username);

                    // Redirect to return URL or dashboard
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Dashboard", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", model.Username);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (IsUserAuthenticated())
            {
                return RedirectToAction("Dashboard", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authenticationService.RegisterAsync(
                    model.Username, 
                    model.Email, 
                    model.Password, 
                    model.FirstName, 
                    model.LastName);

                if (result.Success)
                {
                    _logger.LogInformation("User {Username} registered successfully", model.Username);
                    TempData["SuccessMessage"] = "Registration successful! Please log in with your credentials.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Username}", model.Username);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            try
            {
                var username = HttpContext.Session.GetString("Username");
                
                // Clear session
                HttpContext.Session.Clear();
                
                _logger.LogInformation("User {Username} logged out successfully", username);
                
                TempData["InfoMessage"] = "You have been logged out successfully.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private bool IsUserAuthenticated()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            return !string.IsNullOrEmpty(token);
        }
    }
}