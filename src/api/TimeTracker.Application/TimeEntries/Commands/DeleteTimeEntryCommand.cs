using MediatR;

namespace TimeTracker.Application.TimeEntries.Commands;

public record DeleteTimeEntryCommand(Guid Id) : IRequest<MediatR.Unit>;
