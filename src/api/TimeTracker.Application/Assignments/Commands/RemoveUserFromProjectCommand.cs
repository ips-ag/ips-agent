using MediatR;

namespace TimeTracker.Application.Assignments.Commands;

public record RemoveUserFromProjectCommand(string UserId, string ProjectId) : IRequest<MediatR.Unit>;
