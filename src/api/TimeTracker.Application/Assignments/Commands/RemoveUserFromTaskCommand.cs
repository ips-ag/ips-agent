using MediatR;

namespace TimeTracker.Application.Assignments.Commands;

public record RemoveUserFromTaskCommand(Guid UserId, Guid TaskId) : IRequest<MediatR.Unit>;
