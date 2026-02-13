using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Projects.Queries;

public record GetProjectByIdQuery(Guid Id) : IRequest<ProjectDto>;
