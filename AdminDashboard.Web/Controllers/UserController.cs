using AdminDashboard.Application.DTOs;
using AdminDashboard.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminDashboard.Web.Controllers;

[Authorize]
public class UserController : BaseController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private string GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
    }

    private bool IsMaker()
    {
        var role = GetCurrentUserRole();
        return role == "Maker";
    }

    public async Task<IActionResult> Index()
    {
        if (!Request.Cookies.ContainsKey("AuthToken"))
        {
            return RedirectToAction("Login", "Auth");
        }

        var users = await _userService.GetAllUsersAsync();
        ViewBag.CurrentUserRole = GetCurrentUserRole();
        return View(users);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.CurrentUserRole = GetCurrentUserRole();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CurrentUserRole = GetCurrentUserRole();
            return View(model);
        }

        try
        {
            // Check if user is Maker - create pending record instead
            if (IsMaker())
            {
                await _userService.CreateUserPendingAsync(model, GetCurrentUserId(), GetCurrentUserName());
                TempData["SuccessMessage"] = "User creation request submitted for approval.";
            }
            else
            {
                await _userService.CreateUserAsync(model);
                TempData["SuccessMessage"] = "User created successfully.";
            }
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error creating user: {ex.Message}");
            ViewBag.CurrentUserRole = GetCurrentUserRole();
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            // Check if user is Maker - create pending record instead
            if (IsMaker())
            {
                await _userService.DeleteUserPendingAsync(id, GetCurrentUserId(), GetCurrentUserName());
                TempData["SuccessMessage"] = "User deletion request submitted for approval.";
            }
            else
            {
                await _userService.DeleteUserAsync(id);
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error deleting user: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
