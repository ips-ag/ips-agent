using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Projects.Queries;

public record GetProjectByIdQuery(string Id) : IRequest<ProjectDto>;
