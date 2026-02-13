using MediatR;

namespace TimeTracker.Application.Customers.Commands;

public record ArchiveCustomerCommand(Guid Id) : IRequest<MediatR.Unit>;
