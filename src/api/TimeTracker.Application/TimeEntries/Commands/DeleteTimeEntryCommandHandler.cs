using MediatR;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.TimeEntries.Commands;

public class DeleteTimeEntryCommandHandler : IRequestHandler<DeleteTimeEntryCommand, MediatR.Unit>
{
    private readonly IRepository<TimeEntry> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTimeEntryCommandHandler(IRepository<TimeEntry> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(DeleteTimeEntryCommand request, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(TimeEntry), request.Id);

        await _repository.DeleteAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return MediatR.Unit.Value;
    }
}
