using TimeTracker.Application.Common.Mappings;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Application.DTOs;

public class UnitDto : IMapFrom<Unit>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
