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

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            // Map UserDto to UpdateUserDto for the form
            var updateModel = new UpdateUserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = GetRoleIdFromName(user.RoleName),
                IsActive = user.IsActive
            };

            ViewBag.CurrentUserRole = GetCurrentUserRole();
            ViewBag.UserRoleName = user.RoleName; // Store role name for display
            return View(updateModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading user: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    private int GetRoleIdFromName(string roleName)
    {
        return roleName switch
        {
            "Admin" => 1,
            "Maker" => 2,
            "Checker" => 3,
            _ => 1
        };
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, UpdateUserDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CurrentUserRole = GetCurrentUserRole();
            return View(model);
        }

        try
        {
            model.UserId = id;
            
            // Check if user is Maker - create pending record instead
            if (IsMaker())
            {
                await _userService.UpdateUserPendingAsync(model, GetCurrentUserId(), GetCurrentUserName());
                TempData["SuccessMessage"] = "User update request submitted for approval.";
            }
            else
            {
                await _userService.UpdateUserAsync(model);
                TempData["SuccessMessage"] = "User updated successfully.";
            }
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error updating user: {ex.Message}");
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
