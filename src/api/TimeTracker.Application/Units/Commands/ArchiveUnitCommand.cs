using MediatR;

namespace TimeTracker.Application.Units.Commands;

public record ArchiveUnitCommand(Guid Id) : IRequest<MediatR.Unit>;
