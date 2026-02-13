using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Customers.Queries;

public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto>;
