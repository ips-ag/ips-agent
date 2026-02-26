namespace TimeTracker.Application.DTOs;

public class ProjectReportDto
{
    public string ProjectId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public List<TaskBreakdownDto> TaskBreakdown { get; set; } = new();
    public List<UserBreakdownDto> UserBreakdown { get; set; } = new();
}

public class TaskBreakdownDto
{
    public string TaskName { get; set; } = string.Empty;
    public decimal Hours { get; set; }
}

public class UserBreakdownDto
{
    public string UserName { get; set; } = string.Empty;
    public decimal Hours { get; set; }
}
