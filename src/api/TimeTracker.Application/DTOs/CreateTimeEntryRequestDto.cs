namespace TimeTracker.Application.DTOs;

public class CreateTimeEntryRequestDto
{
    public string UserId { get; init; } = string.Empty;
    public string TaskId { get; init; } = string.Empty;
    public DateOnly Date { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public decimal Hours { get; init; }
    public string? Description { get; init; }
}
