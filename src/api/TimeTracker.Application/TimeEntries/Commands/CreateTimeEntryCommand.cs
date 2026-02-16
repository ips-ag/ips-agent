using MediatR;

namespace TimeTracker.Application.TimeEntries.Commands;

public record CreateTimeEntryCommand(
    Guid UserId,
    Guid TaskId,
    DateOnly Date,
    decimal Hours,
    string? Description) : IRequest<Guid>;
