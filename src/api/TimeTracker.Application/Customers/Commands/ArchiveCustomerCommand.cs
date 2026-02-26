using MediatR;

namespace TimeTracker.Application.Customers.Commands;

public record ArchiveCustomerCommand(string Id) : IRequest<MediatR.Unit>;
