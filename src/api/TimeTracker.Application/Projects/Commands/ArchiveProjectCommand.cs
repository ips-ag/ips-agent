using MediatR;

namespace TimeTracker.Application.Projects.Commands;

public record ArchiveProjectCommand(string Id) : IRequest<MediatR.Unit>;
