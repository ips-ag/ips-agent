namespace TimeTracker.Domain.Entities;

public class ProjectUser
{
    public Guid UserId { get; init; }
    public Guid ProjectId { get; init; }
    public DateTimeOffset AssignedAt { get; set; }
    public Guid? AssignedBy { get; set; }

    public User User { get; set; } = null!;
    public Project Project { get; set; } = null!;
}
