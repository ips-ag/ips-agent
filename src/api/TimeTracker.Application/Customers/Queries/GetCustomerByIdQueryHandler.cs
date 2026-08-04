using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Customers.Queries;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly IRepository<Customer> _repository;

    public GetCustomerByIdQueryHandler(IRepository<Customer> repository)
    {
        _repository = repository;
    }

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken ct)
    {
        var entity = await _repository.Query()
            .Include(c => c.Unit)
            .FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Customer), request.Id);

        return CustomerDto.FromEntity(entity);
    }
}
