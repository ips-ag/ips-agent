using MediatR;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Tasks.Commands;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, MediatR.Unit>
{
    private readonly IRepository<ProjectTask> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskCommandHandler(IRepository<ProjectTask> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(UpdateTaskCommand request, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectTask), request.Id);

        entity.Name = request.Name;
        entity.Code = request.Code;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return MediatR.Unit.Value;
    }
}
