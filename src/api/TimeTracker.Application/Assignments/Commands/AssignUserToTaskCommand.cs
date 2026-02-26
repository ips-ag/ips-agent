using MediatR;

namespace TimeTracker.Application.Assignments.Commands;

public record AssignUserToTaskCommand(string UserId, string TaskId) : IRequest<MediatR.Unit>;
