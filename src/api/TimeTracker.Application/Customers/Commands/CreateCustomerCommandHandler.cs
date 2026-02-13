using MediatR;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Customers.Commands;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Domain.Entities.Unit> _unitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(
        IRepository<Customer> customerRepository,
        IRepository<Domain.Entities.Unit> unitRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitRepository = unitRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        _ = await _unitRepository.GetByIdAsync(request.UnitId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Unit), request.UnitId);

        var entity = new Customer
        {
            Id = Guid.NewGuid(),
            UnitId = request.UnitId,
            Name = request.Name,
            Description = request.Description,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone
        };

        await _customerRepository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return entity.Id;
    }
}
