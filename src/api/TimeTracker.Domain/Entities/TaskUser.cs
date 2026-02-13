namespace TimeTracker.Domain.Entities;

public class TaskUser
{
    public Guid UserId { get; init; }
    public Guid TaskId { get; init; }
    public DateTimeOffset AssignedAt { get; set; }
    public Guid? AssignedBy { get; set; }

    public User User { get; set; } = null!;
    public ProjectTask Task { get; set; } = null!;
}
