namespace TimeTracker.Application.DTOs;

public class TimesheetDto
{
    public DateOnly WeekStart { get; set; }
    public List<TimeEntryDto> Entries { get; set; } = new();
    public decimal TotalHours { get; set; }
}
