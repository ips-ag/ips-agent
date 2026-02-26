using MediatR;

namespace TimeTracker.Application.TimeEntries.Commands;

public record DeleteTimeEntryCommand(string Id) : IRequest<MediatR.Unit>;
