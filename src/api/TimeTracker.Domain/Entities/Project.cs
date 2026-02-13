namespace TimeTracker.Domain.Entities;

public class Project
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public Guid? ParentId { get; init; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Customer Customer { get; set; } = null!;
    public Project? ParentProject { get; set; }
    public ICollection<Project> ChildProjects { get; set; } = new List<Project>();
    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    public ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
}
