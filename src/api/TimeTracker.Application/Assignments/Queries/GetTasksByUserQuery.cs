using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Assignments.Queries;

public record GetTasksByUserQuery(string UserId) : IRequest<List<TaskDto>>;
