using AdminDashboard.Application.DTOs;
using AdminDashboard.Application.Interfaces;
using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;

namespace AdminDashboard.Application.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<UserDto?> GetCurrentUserAsync(int userId);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Username and password are required."
            };
        }

        try
        {
            // Validate dependencies
            if (_passwordHasher == null)
            {
                throw new InvalidOperationException("Password hasher service is not initialized. Please ensure IPasswordHasher is registered in DI container.");
            }

            if (_jwtTokenService == null)
            {
                throw new InvalidOperationException("JWT token service is not initialized. Please ensure IJwtTokenService is registered in DI container.");
            }

            // Get user by username first
            var users = await _userRepository.GetAllAsync();
            Console.WriteLine($"[DEBUG] Total users in DB: {users.Count()}");
            
            var user = users.FirstOrDefault(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase));
            Console.WriteLine($"[DEBUG] Login attempt for username: {request.Username}");
            Console.WriteLine($"[DEBUG] User found: {user != null}");

            if (user == null)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid username or password. (User not found)"
                };
            }

            // Validate user has password hash
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "User account is not properly configured. Please contact administrator."
                };
            }

            Console.WriteLine($"[DEBUG] User PasswordHash length: {user.PasswordHash?.Length ?? 0}");
            Console.WriteLine($"[DEBUG] Input password: {request.Password}");
            
            // Verify password
            bool passwordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash!);
            Console.WriteLine($"[DEBUG] Password verification result: {passwordValid}");
            
            if (!passwordValid)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid username or password. (Password mismatch)"
                };
            }

            if (!user.IsActive)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Your account has been deactivated."
                };
            }

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user);

            return new LoginResponseDto
            {
                Success = true,
                Message = "Login successful.",
                Token = token,
                User = MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = $"Login error: {ex.Message}. Please ensure the database is setup correctly."
            };
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null ? MapToUserDto(user) : null;
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            RoleName = user.RoleName,
            IsActive = user.IsActive
        };
    }
}
