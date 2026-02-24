namespace TimeTracker.Domain.Entities;

public class Unit
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
