using MediatR;

namespace TimeTracker.Application.Units.Commands;

public record UpdateUnitCommand(Guid Id, string Name, string? Description, bool IsActive) : IRequest<MediatR.Unit>;
