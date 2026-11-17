using System.ComponentModel.DataAnnotations;

namespace UserService.Contracts;

public sealed class UserRequest
{
    [Required]
    [StringLength(20, MinimumLength = 3)]
    public required string Username { get; init; }

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public required string Email { get; init; }

    [StringLength(100)]
    public string? FullName { get; init; }

    [Phone]
    [StringLength(15)]
    public string? Phone { get; init; }

    public bool IsActive { get; init; } = true;
}
