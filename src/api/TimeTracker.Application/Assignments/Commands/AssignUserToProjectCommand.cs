using MediatR;

namespace TimeTracker.Application.Assignments.Commands;

public record AssignUserToProjectCommand(string UserId, string ProjectId) : IRequest<MediatR.Unit>;
