using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Tasks.Queries;

public record GetTaskByIdQuery(Guid Id) : IRequest<TaskDto>;
