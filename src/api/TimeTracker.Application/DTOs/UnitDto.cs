using System.Linq.Expressions;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Application.DTOs;

public class UnitDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    internal static Expression<Func<Unit, UnitDto>> Projection { get; } = u => new UnitDto
    {
        Id = u.Id,
        Name = u.Name,
        Description = u.Description,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt
    };

    private static readonly Func<Unit, UnitDto> s_compiled = Projection.Compile();

    internal static UnitDto FromEntity(Unit u) => s_compiled(u);
}
