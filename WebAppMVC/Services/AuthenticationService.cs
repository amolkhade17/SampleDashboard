using WebAppMVC.Models;
using WebAppMVC.Services;
using System.Security.Cryptography;
using System.Text;

namespace WebAppMVC.Services
{
    public interface IAuthenticationService
    {
        Task<(bool Success, string Message, User? User, string? Token)> LoginAsync(string username, string password);
        Task<(bool Success, string Message)> RegisterAsync(string username, string email, string password, string firstName, string lastName);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthenticationService(IUserRepository userRepository, IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<(bool Success, string Message, User? User, string? Token)> LoginAsync(string username, string password)
        {
            try
            {
                var user = await _userRepository.GetUserByUsernameAsync(username);
                
                if (user == null)
                {
                    return (false, "Invalid username or password.", null, null);
                }

                if (!user.IsActive)
                {
                    return (false, "Account is deactivated. Please contact administrator.", null, null);
                }

                if (!VerifyPassword(password, user.PasswordHash))
                {
                    return (false, "Invalid username or password.", null, null);
                }

                // Update last login
                await _userRepository.UpdateLastLoginAsync(user.UserId);
                
                // Generate JWT token
                var token = _jwtTokenService.GenerateToken(user);
                
                return (true, "Login successful.", user, token);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred during login: {ex.Message}", null, null);
            }
        }

        public async Task<(bool Success, string Message)> RegisterAsync(string username, string email, string password, string firstName, string lastName)
        {
            try
            {
                // Check if user already exists
                if (await _userRepository.UserExistsAsync(username, email))
                {
                    return (false, "Username or email already exists.");
                }

                var user = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = HashPassword(password),
                    FirstName = firstName,
                    LastName = lastName,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    Role = "User"
                };

                var result = await _userRepository.CreateUserAsync(user);
                
                if (result)
                {
                    return (true, "Registration successful.");
                }
                else
                {
                    return (false, "Failed to create user account.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred during registration: {ex.Message}");
            }
        }

        public string HashPassword(string password)
        {
            using var hmac = new HMACSHA512();
            var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            var salt = hmac.Key;
            
            // Combine salt and hash
            var hashBytes = new byte[salt.Length + passwordHash.Length];
            Array.Copy(salt, 0, hashBytes, 0, salt.Length);
            Array.Copy(passwordHash, 0, hashBytes, salt.Length, passwordHash.Length);
            
            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                var hashBytes = Convert.FromBase64String(hashedPassword);
                
                // HMACSHA512 uses 128-byte keys and produces 64-byte hashes
                var saltLength = 128; // HMACSHA512 key size
                var hash = new byte[hashBytes.Length - saltLength];
                var salt = new byte[saltLength];
                
                Array.Copy(hashBytes, 0, salt, 0, saltLength);
                Array.Copy(hashBytes, saltLength, hash, 0, hash.Length);
                
                // Compute hash of provided password with extracted salt
                using var hmac = new HMACSHA512(salt);
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                
                // Compare computed hash with stored hash
                return computedHash.SequenceEqual(hash);
            }
            catch
            {
                return false;
            }
        }
    }
}