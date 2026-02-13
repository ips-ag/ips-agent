using MediatR;

namespace TimeTracker.Application.Assignments.Commands;

public record AssignUserToTaskCommand(Guid UserId, Guid TaskId) : IRequest<MediatR.Unit>;
