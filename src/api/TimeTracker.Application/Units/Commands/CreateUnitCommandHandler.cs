using MediatR;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Units.Commands;

public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, string>
{
    private readonly IRepository<Domain.Entities.Unit> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUnitCommandHandler(IRepository<Domain.Entities.Unit> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(CreateUnitCommand request, CancellationToken ct)
    {
        var entity = new Domain.Entities.Unit { Id = Guid.NewGuid().ToString(), Name = request.Name, Description = request.Description };
        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return entity.Id;
    }
}
