using System.Linq.Expressions;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Application.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    internal static Expression<Func<User, UserDto>> Projection { get; } = u => new UserDto
    {
        Id = u.Id,
        Email = u.Email,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Role = u.Role.ToString(),
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt
    };

    private static readonly Func<User, UserDto> s_compiled = Projection.Compile();

    internal static UserDto FromEntity(User u) => s_compiled(u);
}
