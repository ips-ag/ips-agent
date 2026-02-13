using MediatR;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Tasks.Commands;

public class ArchiveTaskCommandHandler : IRequestHandler<ArchiveTaskCommand, MediatR.Unit>
{
    private readonly IRepository<ProjectTask> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveTaskCommandHandler(IRepository<ProjectTask> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(ArchiveTaskCommand request, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectTask), request.Id);

        entity.IsActive = false;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return MediatR.Unit.Value;
    }
}
