using MediatR;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.TimeEntries.Commands;

public class UpdateTimeEntryCommandHandler : IRequestHandler<UpdateTimeEntryCommand, MediatR.Unit>
{
    private readonly IRepository<TimeEntry> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTimeEntryCommandHandler(IRepository<TimeEntry> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(UpdateTimeEntryCommand request, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(TimeEntry), request.Id);

        entity.Date = request.Date;
        entity.Hours = request.Hours;
        entity.Description = request.Description;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return MediatR.Unit.Value;
    }
}
