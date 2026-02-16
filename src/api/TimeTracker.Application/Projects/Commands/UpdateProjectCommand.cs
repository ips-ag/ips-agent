using MediatR;

namespace TimeTracker.Application.Projects.Commands;

public record UpdateProjectCommand(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    DateOnly? StartDate,
    DateOnly? EndDate) : IRequest<MediatR.Unit>;
