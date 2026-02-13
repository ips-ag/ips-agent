using MediatR;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Units.Commands;

public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, Guid>
{
    private readonly IRepository<Domain.Entities.Unit> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUnitCommandHandler(IRepository<Domain.Entities.Unit> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateUnitCommand request, CancellationToken ct)
    {
        var entity = new Domain.Entities.Unit { Id = Guid.NewGuid(), Name = request.Name, Description = request.Description };
        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return entity.Id;
    }
}
