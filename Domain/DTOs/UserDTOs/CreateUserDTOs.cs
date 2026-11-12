namespace Domain.DTOs.UserDTOs;

public class CreateUserDTO
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;      // Password, не PasswordHash (hashing Identity)
    public string PhoneNumber { get; set; } = null!;
    public string Role { get; set; } = null!;          // admin, manager, user
}