namespace TimeTracker.Domain.Entities;

public class TaskUser
{
    public string UserId { get; init; } = string.Empty;
    public string TaskId { get; init; } = string.Empty;
    public DateTimeOffset AssignedAt { get; set; }
    public string? AssignedBy { get; set; }

    public User User { get; set; } = null!;
    public ProjectTask Task { get; set; } = null!;
}
