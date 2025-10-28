using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebAppMVC.Services;
using WebAppMVC.ViewModels;
using WebAppMVC.Models;
using System.Security.Cryptography;
using System.Text;

namespace WebAppMVC.Controllers
{
    public class UsersController : Controller
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IUserRepository _userRepository;

        public UsersController(ILogger<UsersController> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                var userList = users.Select(u => new UserListItemViewModel
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedDate = u.CreatedDate,
                    LastLoginDate = u.LastLoginDate,
                    Department = u.Department,
                    EmployeeId = u.EmployeeId
                }).ToList();

                var viewModel = new UserListViewModel
                {
                    Users = userList,
                    TotalUsers = userList.Count,
                    ActiveUsers = userList.Count(u => u.IsActive)
                };

                ViewBag.OperationTitle = "View Users";
                ViewBag.OperationDescription = "Manage system users and their information";
                ViewBag.OperationIcon = "fas fa-users";
                ViewBag.BackgroundColor = "#2196f3";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users list");
                TempData["ErrorMessage"] = "Error loading users. Please try again.";
                
                var emptyViewModel = new UserListViewModel { Users = new List<UserListItemViewModel>() };
                ViewBag.OperationTitle = "View Users";
                ViewBag.OperationDescription = "Manage system users and their information";
                ViewBag.OperationIcon = "fas fa-users";
                ViewBag.BackgroundColor = "#2196f3";
                
                return View(emptyViewModel);
            }
        }

        public IActionResult Create()
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.OperationTitle = "Add User";
            ViewBag.OperationDescription = "Create a new user in the system";
            ViewBag.OperationIcon = "fas fa-user-plus";
            ViewBag.BackgroundColor = "#4caf50";

            var viewModel = new CreateUserViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.OperationTitle = "Add User";
                ViewBag.OperationDescription = "Create a new user in the system";
                ViewBag.OperationIcon = "fas fa-user-plus";
                ViewBag.BackgroundColor = "#4caf50";
                return View(model);
            }

            try
            {
                // Check if username or email already exists
                if (await _userRepository.UsernameExistsAsync(model.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists");
                    ViewBag.OperationTitle = "Add User";
                    ViewBag.OperationDescription = "Create a new user in the system";
                    ViewBag.OperationIcon = "fas fa-user-plus";
                    ViewBag.BackgroundColor = "#4caf50";
                    return View(model);
                }

                if (await _userRepository.EmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    ViewBag.OperationTitle = "Add User";
                    ViewBag.OperationDescription = "Create a new user in the system";
                    ViewBag.OperationIcon = "fas fa-user-plus";
                    ViewBag.BackgroundColor = "#4caf50";
                    return View(model);
                }

                // Hash the password
                var passwordHash = HashPassword(model.Password);

                // Get current user ID for CreatedBy
                var currentUsername = HttpContext.Session.GetString("Username");
                var currentUser = !string.IsNullOrEmpty(currentUsername) ? 
                    await _userRepository.GetUserByUsernameAsync(currentUsername) : null;

                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = passwordHash,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Role = model.Role,
                    Department = model.Department,
                    EmployeeId = model.EmployeeId,
                    IsActive = model.IsActive,
                    IsEmailConfirmed = model.IsEmailConfirmed,
                    CreatedBy = currentUser?.UserId
                };

                var success = await _userRepository.CreateUserAsync(user);

                if (success)
                {
                    TempData["SuccessMessage"] = $"User '{model.Username}' created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Failed to create user. Please try again.");
                    ViewBag.OperationTitle = "Add User";
                    ViewBag.OperationDescription = "Create a new user in the system";
                    ViewBag.OperationIcon = "fas fa-user-plus";
                    ViewBag.BackgroundColor = "#4caf50";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username}", model.Username);
                ModelState.AddModelError("", "An error occurred while creating the user. Please try again.");
                ViewBag.OperationTitle = "Add User";
                ViewBag.OperationDescription = "Create a new user in the system";
                ViewBag.OperationIcon = "fas fa-user-plus";
                ViewBag.BackgroundColor = "#4caf50";
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new EditUserViewModel
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    Department = user.Department,
                    EmployeeId = user.EmployeeId,
                    IsActive = user.IsActive,
                    IsEmailConfirmed = user.IsEmailConfirmed
                };

                ViewBag.OperationTitle = "Edit User";
                ViewBag.OperationDescription = "Modify existing user information";
                ViewBag.OperationIcon = "fas fa-user-edit";
                ViewBag.BackgroundColor = "#ff9800";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user {UserId} for edit", id);
                TempData["ErrorMessage"] = "Error loading user. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditUserViewModel model)
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != model.UserId)
            {
                TempData["ErrorMessage"] = "Invalid user ID.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.OperationTitle = "Edit User";
                ViewBag.OperationDescription = "Modify existing user information";
                ViewBag.OperationIcon = "fas fa-user-edit";
                ViewBag.BackgroundColor = "#ff9800";
                return View(model);
            }

            try
            {
                var existingUser = await _userRepository.GetUserByIdAsync(id);
                if (existingUser == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if username or email already exists (excluding current user)
                var usernameExists = await _userRepository.UsernameExistsAsync(model.Username);
                var emailExists = await _userRepository.EmailExistsAsync(model.Email);

                if (usernameExists && existingUser.Username != model.Username)
                {
                    ModelState.AddModelError("Username", "Username already exists");
                    ViewBag.OperationTitle = "Edit User";
                    ViewBag.OperationDescription = "Modify existing user information";
                    ViewBag.OperationIcon = "fas fa-user-edit";
                    ViewBag.BackgroundColor = "#ff9800";
                    return View(model);
                }

                if (emailExists && existingUser.Email != model.Email)
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    ViewBag.OperationTitle = "Edit User";
                    ViewBag.OperationDescription = "Modify existing user information";
                    ViewBag.OperationIcon = "fas fa-user-edit";
                    ViewBag.BackgroundColor = "#ff9800";
                    return View(model);
                }

                // Get current user ID for LastModifiedBy
                var currentUsername = HttpContext.Session.GetString("Username");
                var currentUser = !string.IsNullOrEmpty(currentUsername) ? 
                    await _userRepository.GetUserByUsernameAsync(currentUsername) : null;

                // Update user properties
                existingUser.Username = model.Username;
                existingUser.Email = model.Email;
                existingUser.FirstName = model.FirstName;
                existingUser.LastName = model.LastName;
                existingUser.PhoneNumber = model.PhoneNumber;
                existingUser.Role = model.Role;
                existingUser.Department = model.Department;
                existingUser.EmployeeId = model.EmployeeId;
                existingUser.IsActive = model.IsActive;
                existingUser.IsEmailConfirmed = model.IsEmailConfirmed;
                existingUser.LastModifiedBy = currentUser?.UserId;

                // Update password if provided
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    existingUser.PasswordHash = HashPassword(model.NewPassword);
                }

                var success = await _userRepository.UpdateUserAsync(existingUser);

                if (success)
                {
                    TempData["SuccessMessage"] = $"User '{model.Username}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update user. Please try again.");
                    ViewBag.OperationTitle = "Edit User";
                    ViewBag.OperationDescription = "Modify existing user information";
                    ViewBag.OperationIcon = "fas fa-user-edit";
                    ViewBag.BackgroundColor = "#ff9800";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                ModelState.AddModelError("", "An error occurred while updating the user. Please try again.");
                ViewBag.OperationTitle = "Edit User";
                ViewBag.OperationDescription = "Modify existing user information";
                ViewBag.OperationIcon = "fas fa-user-edit";
                ViewBag.BackgroundColor = "#ff9800";
                return View(model);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new UserDetailsViewModel
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    Department = user.Department,
                    EmployeeId = user.EmployeeId,
                    IsActive = user.IsActive,
                    IsEmailConfirmed = user.IsEmailConfirmed,
                    CreatedDate = user.CreatedDate,
                    LastLoginDate = user.LastLoginDate,
                    LastModifiedDate = user.LastModifiedDate,
                    ProfileImageUrl = user.ProfileImageUrl
                };

                ViewBag.OperationTitle = "User Details";
                ViewBag.OperationDescription = "View detailed user information";
                ViewBag.OperationIcon = "fas fa-user";
                ViewBag.BackgroundColor = "#2196f3";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user details {UserId}", id);
                TempData["ErrorMessage"] = "Error loading user details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new DeleteUserViewModel
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role,
                    CreatedDate = user.CreatedDate,
                    LastLoginDate = user.LastLoginDate
                };

                ViewBag.OperationTitle = "Delete User";
                ViewBag.OperationDescription = "Remove user from the system";
                ViewBag.OperationIcon = "fas fa-user-times";
                ViewBag.BackgroundColor = "#f44336";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user {UserId} for delete", id);
                TempData["ErrorMessage"] = "Error loading user. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Check if user exists
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Don't allow deletion of current user
                var currentUsername = HttpContext.Session.GetString("Username");
                if (!string.IsNullOrEmpty(currentUsername) && currentUsername == user.Username)
                {
                    TempData["ErrorMessage"] = "You cannot delete your own account.";
                    return RedirectToAction(nameof(Index));
                }

                var success = await _userRepository.DeleteUserAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = $"User '{user.Username}' deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete user. Please try again.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the user. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool IsUserAuthenticated()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            return !string.IsNullOrEmpty(token);
        }

        private static string HashPassword(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                var salt = hmac.Key;
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                
                // Combine salt and hash
                var combined = new byte[salt.Length + hash.Length];
                Array.Copy(salt, 0, combined, 0, salt.Length);
                Array.Copy(hash, 0, combined, salt.Length, hash.Length);
                
                return Convert.ToBase64String(combined);
            }
        }
    }
}