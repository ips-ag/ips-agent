using TimeTracker.Domain.Enums;

namespace TimeTracker.Domain.Entities;

public class User
{
    public Guid Id { get; init; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
    public ICollection<TaskUser> TaskUsers { get; set; } = new List<TaskUser>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}
