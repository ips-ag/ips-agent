using MediatR;

namespace TimeTracker.Application.Customers.Commands;

public record CreateCustomerCommand(
    string UnitId,
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone) : IRequest<string>;
