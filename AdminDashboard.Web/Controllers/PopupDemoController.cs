using AdminDashboard.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard.Web.Controllers;

[Authorize]
public class PopupDemoController : Controller
{
    private readonly IUserService _userService;

    public PopupDemoController(IUserService userService)
    {
        _userService = userService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult SubmitForm(string name, string email, string message)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
        {
            return Json(new { success = false, message = "Name and Email are required!" });
        }

        // Simulate processing
        return Json(new { success = true, message = $"Form submitted successfully! Welcome, {name}!" });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteItem(int id)
    {
        try
        {
            var rowsAffected = await _userService.DeleteUserAsync(id);
            if (rowsAffected > 0)
            {
                return Json(new { success = true, message = "User deleted successfully!" });
            }
            return Json(new { success = false, message = "Failed to delete user. User not found." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error deleting user: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(string search = "")
    {
        try
        {
            var allUsers = await _userService.GetAllUsersAsync();
            
            var users = allUsers.Select(u => new
            {
                Id = u.UserId,
                Name = u.FullName,
                Email = u.Email,
                Role = u.RoleName,
                Status = u.IsActive ? "Active" : "Inactive"
            });

            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u => 
                    u.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Role.Contains(search, StringComparison.OrdinalIgnoreCase)
                );
            }

            return Json(new { success = true, data = users });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error loading users: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ActivateUser(int id)
    {
        try
        {
            var success = await _userService.ActivateUserAsync(id);
            if (success)
            {
                return Json(new { success = true, message = "User activated successfully!" });
            }
            return Json(new { success = false, message = "Failed to activate user. User not found." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error activating user: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        try
        {
            var success = await _userService.DeactivateUserAsync(id);
            if (success)
            {
                return Json(new { success = true, message = "User deactivated successfully!" });
            }
            return Json(new { success = false, message = "Failed to deactivate user. User not found." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error deactivating user: {ex.Message}" });
        }
    }

    [HttpPost]
    public IActionResult SendEmail(int userId, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
        {
            return Json(new { success = false, message = "Subject and Body are required!" });
        }

        return Json(new { success = true, message = $"Email sent successfully to User #{userId}!" });
    }
}
