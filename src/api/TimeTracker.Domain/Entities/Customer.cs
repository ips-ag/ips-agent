namespace TimeTracker.Domain.Entities;

public class Customer
{
    public string Id { get; init; } = string.Empty;
    public string UnitId { get; init; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Unit Unit { get; set; } = null!;
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
