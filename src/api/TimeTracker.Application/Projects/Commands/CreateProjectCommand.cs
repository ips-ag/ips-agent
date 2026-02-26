using MediatR;

namespace TimeTracker.Application.Projects.Commands;

public record CreateProjectCommand(
    string CustomerId,
    string? ParentId,
    string Name,
    string Code,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate) : IRequest<string>;
