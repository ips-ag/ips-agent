using MediatR;

namespace TimeTracker.Application.Tasks.Commands;

public record UpdateTaskCommand(
    string Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    DateOnly? StartDate,
    DateOnly? EndDate) : IRequest<MediatR.Unit>;
