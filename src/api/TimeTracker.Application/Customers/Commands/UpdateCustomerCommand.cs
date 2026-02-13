using MediatR;

namespace TimeTracker.Application.Customers.Commands;

public record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone,
    bool IsActive) : IRequest<MediatR.Unit>;
