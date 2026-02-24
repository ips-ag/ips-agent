namespace TimeTracker.Domain.Entities;

public class ProjectUser
{
    public string UserId { get; init; } = string.Empty;
    public string ProjectId { get; init; } = string.Empty;
    public DateTimeOffset AssignedAt { get; set; }
    public string? AssignedBy { get; set; }

    public User User { get; set; } = null!;
    public Project Project { get; set; } = null!;
}
