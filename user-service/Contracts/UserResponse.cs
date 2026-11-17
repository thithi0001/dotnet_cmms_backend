using UserService.Models;

namespace UserService.Contracts;

public sealed record UserResponse(
    int Id,
    string Username,
    string Email,
    string? FullName,
    string? Phone,
    bool IsActive,
    DateTimeOffset CreatedAt)
{
    public static UserResponse From(User user) =>
        new(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.Phone,
            user.IsActive,
            user.CreatedAt);
}
