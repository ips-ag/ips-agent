namespace TimeTracker.Domain.Entities;

public class ProjectTask
{
    public string Id { get; init; } = string.Empty;
    public string ProjectId { get; init; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Project Project { get; set; } = null!;
    public ICollection<TaskUser> TaskUsers { get; set; } = new List<TaskUser>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}
