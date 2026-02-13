using MediatR;

namespace TimeTracker.Application.Assignments.Commands;

public record RemoveUserFromProjectCommand(Guid UserId, Guid ProjectId) : IRequest<MediatR.Unit>;
