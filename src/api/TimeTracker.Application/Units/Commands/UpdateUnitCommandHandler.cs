using MediatR;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Units.Commands;

public class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand, MediatR.Unit>
{
    private readonly IRepository<Domain.Entities.Unit> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUnitCommandHandler(IRepository<Domain.Entities.Unit> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(UpdateUnitCommand request, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Unit), request.Id);

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return MediatR.Unit.Value;
    }
}
