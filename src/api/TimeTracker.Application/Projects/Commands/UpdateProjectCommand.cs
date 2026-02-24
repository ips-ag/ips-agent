using MediatR;

namespace TimeTracker.Application.Projects.Commands;

public record UpdateProjectCommand(
    string Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    DateOnly? StartDate,
    DateOnly? EndDate) : IRequest<MediatR.Unit>;
