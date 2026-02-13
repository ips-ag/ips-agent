using MediatR;

namespace TimeTracker.Application.Projects.Commands;

public record ArchiveProjectCommand(Guid Id) : IRequest<MediatR.Unit>;
