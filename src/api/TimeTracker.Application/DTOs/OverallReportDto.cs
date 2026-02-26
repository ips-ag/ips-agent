namespace TimeTracker.Application.DTOs;

public class OverallReportDto
{
    public decimal TotalHours { get; set; }
    public List<ProjectBreakdownDto> ProjectBreakdown { get; set; } = new();
    public List<UserBreakdownDto> UserBreakdown { get; set; } = new();
}
