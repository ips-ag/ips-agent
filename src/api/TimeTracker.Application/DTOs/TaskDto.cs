using System.Linq.Expressions;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Application.DTOs;

public class TaskDto
{
    public string Id { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string? ProjectName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    internal static Expression<Func<ProjectTask, TaskDto>> Projection { get; } = t => new TaskDto
    {
        Id = t.Id,
        ProjectId = t.ProjectId,
        ProjectName = t.Project.Name,
        Name = t.Name,
        Code = t.Code,
        Description = t.Description,
        IsActive = t.IsActive,
        StartDate = t.StartDate.HasValue ? t.StartDate.Value.ToString("yyyy-MM-dd") : null,
        EndDate = t.EndDate.HasValue ? t.EndDate.Value.ToString("yyyy-MM-dd") : null,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt
    };

    private static readonly Func<ProjectTask, TaskDto> s_compiled = Projection.Compile();

    internal static TaskDto FromEntity(ProjectTask t) => s_compiled(t);
}
