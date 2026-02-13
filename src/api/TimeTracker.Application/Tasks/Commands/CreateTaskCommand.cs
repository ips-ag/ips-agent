using MediatR;

namespace TimeTracker.Application.Tasks.Commands;

public record CreateTaskCommand(
    Guid ProjectId,
    string Name,
    string Code,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate) : IRequest<Guid>;
