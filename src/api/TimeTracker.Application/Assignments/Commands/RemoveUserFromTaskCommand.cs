using MediatR;

namespace TimeTracker.Application.Assignments.Commands;

public record RemoveUserFromTaskCommand(string UserId, string TaskId) : IRequest<MediatR.Unit>;
