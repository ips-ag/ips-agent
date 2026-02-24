using MediatR;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Projects.Commands;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, string>
{
    private readonly IRepository<Project> _projectRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProjectCommandHandler(
        IRepository<Project> projectRepository,
        IRepository<Customer> customerRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(CreateProjectCommand request, CancellationToken ct)
    {
        _ = await _customerRepository.GetByIdAsync(request.CustomerId, ct)
            ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

        if (request.ParentId != null)
        {
            _ = await _projectRepository.GetByIdAsync(request.ParentId, ct)
                ?? throw new NotFoundException(nameof(Project), request.ParentId);
        }

        var entity = new Project
        {
            Id = Guid.NewGuid().ToString(),
            CustomerId = request.CustomerId,
            ParentId = request.ParentId,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _projectRepository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return entity.Id;
    }
}
