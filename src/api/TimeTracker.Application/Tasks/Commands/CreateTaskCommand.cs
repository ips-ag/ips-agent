using MediatR;

namespace TimeTracker.Application.Tasks.Commands;

public record CreateTaskCommand(
    string ProjectId,
    string Name,
    string Code,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate) : IRequest<string>;
