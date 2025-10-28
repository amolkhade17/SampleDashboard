using System.ComponentModel.DataAnnotations;

namespace WebAppMVC.Models
{
    public class User
    {
        public int UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public bool IsEmailConfirmed { get; set; } = false;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginDate { get; set; }
        
        public DateTime? LastModifiedDate { get; set; }
        
        [StringLength(20)]
        public string Role { get; set; } = "User";
        
        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }
        
        [StringLength(100)]
        public string? Department { get; set; }
        
        [StringLength(50)]
        public string? EmployeeId { get; set; }
        
        public int? CreatedBy { get; set; }
        
        public int? LastModifiedBy { get; set; }

        // Computed property
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}