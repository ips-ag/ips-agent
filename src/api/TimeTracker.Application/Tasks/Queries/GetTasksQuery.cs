using MediatR;
using TimeTracker.Application.Common.Models;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Tasks.Queries;

public record GetTasksQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    Guid? ProjectId = null) : IRequest<PagedList<TaskDto>>;
