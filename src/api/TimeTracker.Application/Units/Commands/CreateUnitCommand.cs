using MediatR;

namespace TimeTracker.Application.Units.Commands;

public record CreateUnitCommand(string Name, string? Description) : IRequest<Guid>;
