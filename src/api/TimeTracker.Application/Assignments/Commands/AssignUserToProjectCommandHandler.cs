using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Assignments.Commands;

public class AssignUserToProjectCommandHandler : IRequestHandler<AssignUserToProjectCommand, MediatR.Unit>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Project> _projectRepository;
    private readonly IRepository<ProjectUser> _projectUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignUserToProjectCommandHandler(
        IRepository<User> userRepository,
        IRepository<Project> projectRepository,
        IRepository<ProjectUser> projectUserRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _projectRepository = projectRepository;
        _projectUserRepository = projectUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(AssignUserToProjectCommand request, CancellationToken ct)
    {
        _ = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        _ = await _projectRepository.GetByIdAsync(request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var existing = await _projectUserRepository.Query()
            .FirstOrDefaultAsync(pu => pu.UserId == request.UserId && pu.ProjectId == request.ProjectId, ct);

        if (existing != null)
        {
            return MediatR.Unit.Value; // Already assigned
        }

        var entity = new ProjectUser
        {
            UserId = request.UserId,
            ProjectId = request.ProjectId,
            AssignedAt = DateTimeOffset.UtcNow
        };

        await _projectUserRepository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return MediatR.Unit.Value;
    }
}
