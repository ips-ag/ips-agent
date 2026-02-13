using MediatR;

namespace TimeTracker.Application.Customers.Commands;

public record CreateCustomerCommand(
    Guid UnitId,
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone) : IRequest<Guid>;
