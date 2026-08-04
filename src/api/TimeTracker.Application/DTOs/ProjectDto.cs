using System.Linq.Expressions;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Application.DTOs;

public class ProjectDto
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<ProjectDto>? Children { get; set; }

    internal static Expression<Func<Project, ProjectDto>> Projection { get; } = p => new ProjectDto
    {
        Id = p.Id,
        CustomerId = p.CustomerId,
        CustomerName = p.Customer.Name,
        ParentId = p.ParentId,
        Name = p.Name,
        Code = p.Code,
        Description = p.Description,
        IsActive = p.IsActive,
        StartDate = p.StartDate.HasValue ? p.StartDate.Value.ToString("yyyy-MM-dd") : null,
        EndDate = p.EndDate.HasValue ? p.EndDate.Value.ToString("yyyy-MM-dd") : null,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    private static readonly Func<Project, ProjectDto> s_compiled = Projection.Compile();

    internal static ProjectDto FromEntity(Project p) => s_compiled(p);
}
