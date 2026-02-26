using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Units.Commands;

public class ArchiveUnitCommandHandler : IRequestHandler<ArchiveUnitCommand, MediatR.Unit>
{
    private readonly IRepository<Domain.Entities.Unit> _unitRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveUnitCommandHandler(
        IRepository<Domain.Entities.Unit> unitRepository,
        IRepository<Customer> customerRepository,
        IUnitOfWork unitOfWork)
    {
        _unitRepository = unitRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(ArchiveUnitCommand request, CancellationToken ct)
    {
        var entity = await _unitRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Unit), request.Id);

        entity.IsActive = false;
        await _unitRepository.UpdateAsync(entity, ct);

        var customers = await _customerRepository.Query()
            .Where(c => c.UnitId == request.Id && c.IsActive)
            .ToListAsync(ct);

        foreach (var customer in customers)
        {
            customer.IsActive = false;
            await _customerRepository.UpdateAsync(customer, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return MediatR.Unit.Value;
    }
}
