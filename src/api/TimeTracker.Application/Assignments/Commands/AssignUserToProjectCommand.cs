using MediatR;

namespace TimeTracker.Application.Assignments.Commands;

public record AssignUserToProjectCommand(Guid UserId, Guid ProjectId) : IRequest<MediatR.Unit>;
