using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Customers.Commands;

public class ArchiveCustomerCommandHandler : IRequestHandler<ArchiveCustomerCommand, MediatR.Unit>
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Project> _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveCustomerCommandHandler(
        IRepository<Customer> customerRepository,
        IRepository<Project> projectRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(ArchiveCustomerCommand request, CancellationToken ct)
    {
        var entity = await _customerRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Customer), request.Id);

        entity.IsActive = false;
        await _customerRepository.UpdateAsync(entity, ct);

        var projects = await _projectRepository.Query()
            .Where(p => p.CustomerId == request.Id && p.IsActive)
            .ToListAsync(ct);

        foreach (var project in projects)
        {
            project.IsActive = false;
            await _projectRepository.UpdateAsync(project, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return MediatR.Unit.Value;
    }
}
