namespace TimeTracker.Application.DTOs;

public class UserReportDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public List<ProjectBreakdownDto> ProjectBreakdown { get; set; } = new();
}

public class ProjectBreakdownDto
{
    public string ProjectName { get; set; } = string.Empty;
    public decimal Hours { get; set; }
}
