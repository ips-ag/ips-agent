using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Customers.Queries;

public record GetCustomerByIdQuery(string Id) : IRequest<CustomerDto>;
