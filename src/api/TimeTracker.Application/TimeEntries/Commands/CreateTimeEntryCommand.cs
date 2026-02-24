using MediatR;

namespace TimeTracker.Application.TimeEntries.Commands;

public record CreateTimeEntryCommand(
    string UserId,
    string TaskId,
    DateOnly Date,
    decimal Hours,
    string? Description) : IRequest<string>;
