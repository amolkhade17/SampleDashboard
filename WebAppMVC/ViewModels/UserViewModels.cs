using System.ComponentModel.DataAnnotations;

namespace WebAppMVC.ViewModels
{
    // List View Models
    public class UserListViewModel
    {
        public List<UserListItemViewModel> Users { get; set; } = new List<UserListItemViewModel>();
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
    }

    public class UserListItemViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? Department { get; set; }
        public string? EmployeeId { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
        public string StatusBadge => IsActive ? "badge bg-success" : "badge bg-secondary";
        public string StatusText => IsActive ? "Active" : "Inactive";
    }

    // Create User View Model
    public class CreateUserViewModel
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Role")]
        public string Role { get; set; } = "User";

        [StringLength(100)]
        [Display(Name = "Department")]
        public string? Department { get; set; }

        [StringLength(50)]
        [Display(Name = "Employee ID")]
        public string? EmployeeId { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Email Confirmed")]
        public bool IsEmailConfirmed { get; set; } = false;
    }

    // Edit User View Model
    public class EditUserViewModel
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password (Leave blank to keep current)")]
        public string? NewPassword { get; set; }

        [Compare("NewPassword")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public string? ConfirmNewPassword { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Role")]
        public string Role { get; set; } = "User";

        [StringLength(100)]
        [Display(Name = "Department")]
        public string? Department { get; set; }

        [StringLength(50)]
        [Display(Name = "Employee ID")]
        public string? EmployeeId { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Email Confirmed")]
        public bool IsEmailConfirmed { get; set; } = false;
    }

    // User Details View Model
    public class UserDetailsViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? EmployeeId { get; set; }
        public bool IsActive { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string? ProfileImageUrl { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
        public string StatusBadge => IsActive ? "badge bg-success" : "badge bg-secondary";
        public string StatusText => IsActive ? "Active" : "Inactive";
        public string EmailStatusBadge => IsEmailConfirmed ? "badge bg-success" : "badge bg-warning";
        public string EmailStatusText => IsEmailConfirmed ? "Confirmed" : "Not Confirmed";
    }

    // Delete User View Model
    public class DeleteUserViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}