using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Assignments.Queries;

public record GetTasksByUserQuery(Guid UserId) : IRequest<List<TaskDto>>;
