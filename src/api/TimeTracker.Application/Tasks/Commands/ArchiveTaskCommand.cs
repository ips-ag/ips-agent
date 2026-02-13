using MediatR;

namespace TimeTracker.Application.Tasks.Commands;

public record ArchiveTaskCommand(Guid Id) : IRequest<MediatR.Unit>;
