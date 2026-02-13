using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Assignments.Commands;

public class RemoveUserFromProjectCommandHandler : IRequestHandler<RemoveUserFromProjectCommand, MediatR.Unit>
{
    private readonly IRepository<ProjectUser> _projectUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveUserFromProjectCommandHandler(
        IRepository<ProjectUser> projectUserRepository,
        IUnitOfWork unitOfWork)
    {
        _projectUserRepository = projectUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(RemoveUserFromProjectCommand request, CancellationToken ct)
    {
        var entity = await _projectUserRepository.Query()
            .FirstOrDefaultAsync(pu => pu.UserId == request.UserId && pu.ProjectId == request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(ProjectUser), $"UserId={request.UserId}, ProjectId={request.ProjectId}");

        await _projectUserRepository.DeleteAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return MediatR.Unit.Value;
    }
}
