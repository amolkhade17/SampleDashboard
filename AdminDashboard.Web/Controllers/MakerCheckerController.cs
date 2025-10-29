using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminDashboard.Application.Services;
using AdminDashboard.Application.DTOs;
using System.Security.Claims;
using System.Text.Json;

namespace AdminDashboard.Web.Controllers;

[Authorize]
public class MakerCheckerController : BaseController
{
    private readonly IPendingRecordService _pendingRecordService;
    private readonly IUserService _userService;

    public MakerCheckerController(IPendingRecordService pendingRecordService, IUserService userService)
    {
        _pendingRecordService = pendingRecordService;
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

    // GET: MakerChecker/PendingList
    public async Task<IActionResult> PendingList()
    {
        var records = await _pendingRecordService.GetAllAsync("Pending");
        return View(records);
    }

    // GET: MakerChecker/ApprovedList
    public async Task<IActionResult> ApprovedList()
    {
        var records = await _pendingRecordService.GetAllAsync("Approved");
        return View(records);
    }

    // GET: MakerChecker/RejectedList
    public async Task<IActionResult> RejectedList()
    {
        var records = await _pendingRecordService.GetAllAsync("Rejected");
        return View(records);
    }

    // GET: MakerChecker/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var record = await _pendingRecordService.GetByIdAsync(id);
        if (record == null)
        {
            TempData["ErrorMessage"] = "Record not found";
            return RedirectToAction(nameof(PendingList));
        }

        // Parse the JSON data for display
        try
        {
            ViewBag.ParsedData = JsonSerializer.Deserialize<Dictionary<string, object>>(record.RecordData);
        }
        catch
        {
            ViewBag.ParsedData = new Dictionary<string, object>();
        }

        ViewBag.CurrentUserId = GetCurrentUserId();
        ViewBag.CurrentUserRole = GetCurrentUserRole();

        return View(record);
    }

    // POST: MakerChecker/Approve
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(ApproveRejectDto model)
    {
        var currentUserId = GetCurrentUserId();
        var currentUserName = GetCurrentUserName();
        var currentUserRole = GetCurrentUserRole();

        // Check if user has Checker role
        if (currentUserRole != "Checker" && currentUserRole != "Admin")
        {
            TempData["ErrorMessage"] = "Only Checker or Admin can approve records";
            return RedirectToAction(nameof(Details), new { id = model.PendingId });
        }

        var (success, message) = await _pendingRecordService.ApproveAsync(
            model.PendingId,
            currentUserId,
            currentUserName,
            model.Comments
        );

        if (success)
        {
            // Execute the approved operation
            var (execSuccess, execMessage) = await _pendingRecordService.ExecuteApprovedOperationAsync(model.PendingId);
            
            if (execSuccess)
            {
                TempData["SuccessMessage"] = "Record approved and executed successfully";
            }
            else
            {
                TempData["WarningMessage"] = $"Record approved but execution failed: {execMessage}";
            }
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(PendingList));
    }

    // POST: MakerChecker/Reject
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(ApproveRejectDto model)
    {
        var currentUserId = GetCurrentUserId();
        var currentUserName = GetCurrentUserName();
        var currentUserRole = GetCurrentUserRole();

        // Check if user has Checker role
        if (currentUserRole != "Checker" && currentUserRole != "Admin")
        {
            TempData["ErrorMessage"] = "Only Checker or Admin can reject records";
            return RedirectToAction(nameof(Details), new { id = model.PendingId });
        }

        if (string.IsNullOrWhiteSpace(model.Comments))
        {
            TempData["ErrorMessage"] = "Comments are required when rejecting a record";
            return RedirectToAction(nameof(Details), new { id = model.PendingId });
        }

        var (success, message) = await _pendingRecordService.RejectAsync(
            model.PendingId,
            currentUserId,
            currentUserName,
            model.Comments
        );

        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(PendingList));
    }
}
