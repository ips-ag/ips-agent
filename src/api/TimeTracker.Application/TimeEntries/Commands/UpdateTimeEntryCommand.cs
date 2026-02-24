using MediatR;

namespace TimeTracker.Application.TimeEntries.Commands;

public record UpdateTimeEntryCommand(
    string Id,
    string TaskId,
    DateOnly Date,
    decimal Hours,
    string? Description) : IRequest<MediatR.Unit>;
