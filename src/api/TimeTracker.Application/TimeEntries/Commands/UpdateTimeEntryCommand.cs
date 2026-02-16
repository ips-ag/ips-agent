using MediatR;

namespace TimeTracker.Application.TimeEntries.Commands;

public record UpdateTimeEntryCommand(
    Guid Id,
    Guid TaskId,
    DateOnly Date,
    decimal Hours,
    string? Description) : IRequest<MediatR.Unit>;
