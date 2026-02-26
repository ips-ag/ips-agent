using MediatR;

namespace TimeTracker.Application.Customers.Commands;

public record UpdateCustomerCommand(
    string Id,
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone,
    bool IsActive) : IRequest<MediatR.Unit>;
