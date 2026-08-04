using System.Linq.Expressions;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Application.DTOs;

public class TimeEntryDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string TaskId { get; set; } = string.Empty;
    public string? TaskName { get; set; }
    public string? ProjectName { get; set; }
    public string Date { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    internal static Expression<Func<TimeEntry, TimeEntryDto>> Projection { get; } = e => new TimeEntryDto
    {
        Id = e.Id,
        UserId = e.UserId,
        UserName = e.User.FirstName + " " + e.User.LastName,
        TaskId = e.TaskId,
        TaskName = e.Task.Name,
        ProjectName = e.Task.Project.Name,
        Date = e.Date.ToString("yyyy-MM-dd"),
        Hours = e.Hours,
        Description = e.Description,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private static readonly Func<TimeEntry, TimeEntryDto> s_compiled = Projection.Compile();

    internal static TimeEntryDto FromEntity(TimeEntry e) => s_compiled(e);
}
