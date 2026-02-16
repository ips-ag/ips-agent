using MediatR;

namespace TimeTracker.Application.Projects.Commands;

public record CreateProjectCommand(
    Guid CustomerId,
    Guid? ParentId,
    string Name,
    string Code,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate) : IRequest<Guid>;
