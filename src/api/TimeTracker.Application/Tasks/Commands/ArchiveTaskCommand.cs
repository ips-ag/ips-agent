using MediatR;

namespace TimeTracker.Application.Tasks.Commands;

public record ArchiveTaskCommand(string Id) : IRequest<MediatR.Unit>;
