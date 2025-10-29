using AdminDashboard.Application.DTOs;
using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;
using System.Text.Json;

namespace AdminDashboard.Application.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<int> CreateUserAsync(CreateUserDto dto);
    Task<int> UpdateUserAsync(UpdateUserDto dto);
    Task<int> DeleteUserAsync(int userId);
    Task<bool> ChangePasswordAsync(ChangePasswordDto dto);
    Task<int> CreateUserPendingAsync(CreateUserDto dto, int makerId, string makerName);
    Task<int> UpdateUserPendingAsync(UpdateUserDto dto, int makerId, string makerName);
    Task<int> DeleteUserPendingAsync(int userId, int makerId, string makerName);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPendingRecordRepository _pendingRecordRepository;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, IPendingRecordRepository pendingRecordRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _pendingRecordRepository = pendingRecordRepository;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => new UserDto
        {
            UserId = u.UserId,
            Username = u.Username,
            Email = u.Email,
            FullName = u.FullName,
            RoleName = u.RoleName,
            IsActive = u.IsActive
        });
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

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

    public async Task<int> CreateUserAsync(CreateUserDto dto)
    {
        var user = new User
        {
            Username = dto.Username,
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            Email = dto.Email,
            FullName = dto.FullName,
            RoleId = dto.RoleId,
            IsActive = dto.IsActive
        };

        return await _userRepository.CreateAsync(user);
    }

    public async Task<int> UpdateUserAsync(UpdateUserDto dto)
    {
        var user = new User
        {
            UserId = dto.UserId,
            Username = dto.Username,
            Email = dto.Email,
            FullName = dto.FullName,
            RoleId = dto.RoleId,
            IsActive = dto.IsActive
        };

        return await _userRepository.UpdateAsync(user);
    }

    public async Task<int> DeleteUserAsync(int userId)
    {
        return await _userRepository.DeleteAsync(userId);
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(dto.UserId);
        if (user == null) return false;

        // Verify current password
        if (!_passwordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            return false;

        // Hash new password
        var newPasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
        
        var result = await _userRepository.ChangePasswordAsync(dto.UserId, newPasswordHash);
        return result > 0;
    }

    // Maker-Checker Methods
    public async Task<int> CreateUserPendingAsync(CreateUserDto dto, int makerId, string makerName)
    {
        var userData = new
        {
            Username = dto.Username,
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            Email = dto.Email,
            FullName = dto.FullName,
            RoleId = dto.RoleId,
            IsActive = dto.IsActive
        };

        var pendingRecord = new PendingRecord
        {
            RecordType = "User",
            Operation = "Create",
            RecordData = JsonSerializer.Serialize(userData),
            MakerId = makerId,
            MakerName = makerName,
            Status = "Pending",
            CreatedDate = DateTime.Now
        };

        return await _pendingRecordRepository.CreateAsync(pendingRecord);
    }

    public async Task<int> UpdateUserPendingAsync(UpdateUserDto dto, int makerId, string makerName)
    {
        var userData = new
        {
            Username = dto.Username,
            Email = dto.Email,
            FullName = dto.FullName,
            RoleId = dto.RoleId,
            IsActive = dto.IsActive
        };

        var pendingRecord = new PendingRecord
        {
            RecordType = "User",
            Operation = "Update",
            RecordId = dto.UserId,
            RecordData = JsonSerializer.Serialize(userData),
            MakerId = makerId,
            MakerName = makerName,
            Status = "Pending",
            CreatedDate = DateTime.Now
        };

        return await _pendingRecordRepository.CreateAsync(pendingRecord);
    }

    public async Task<int> DeleteUserPendingAsync(int userId, int makerId, string makerName)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return 0;

        var userData = new
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName
        };

        var pendingRecord = new PendingRecord
        {
            RecordType = "User",
            Operation = "Delete",
            RecordId = userId,
            RecordData = JsonSerializer.Serialize(userData),
            MakerId = makerId,
            MakerName = makerName,
            Status = "Pending",
            CreatedDate = DateTime.Now
        };

        return await _pendingRecordRepository.CreateAsync(pendingRecord);
    }
}
