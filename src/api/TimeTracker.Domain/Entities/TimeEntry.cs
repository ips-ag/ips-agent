namespace TimeTracker.Domain.Entities;

public class TimeEntry
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid TaskId { get; init; }
    public DateOnly Date { get; set; }
    public decimal Hours { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ProjectTask Task { get; set; } = null!;
}
