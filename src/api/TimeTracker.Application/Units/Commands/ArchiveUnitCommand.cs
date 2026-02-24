using MediatR;

namespace TimeTracker.Application.Units.Commands;

public record ArchiveUnitCommand(string Id) : IRequest<MediatR.Unit>;
