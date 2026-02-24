using MediatR;

namespace TimeTracker.Application.Units.Commands;

public record UpdateUnitCommand(string Id, string Name, string? Description, bool IsActive) : IRequest<MediatR.Unit>;
