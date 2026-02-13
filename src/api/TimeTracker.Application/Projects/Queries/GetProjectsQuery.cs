using MediatR;
using TimeTracker.Application.Common.Models;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Projects.Queries;

public record GetProjectsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    Guid? CustomerId = null) : IRequest<PagedList<ProjectDto>>;
