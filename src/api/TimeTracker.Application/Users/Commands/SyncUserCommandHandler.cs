using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Users.Commands;

public class SyncUserCommandHandler : IRequestHandler<SyncUserCommand, string>
{
    private readonly IRepository<User> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SyncUserCommandHandler(IRepository<User> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(SyncUserCommand request, CancellationToken ct)
    {
        var existingUser = await _repository.Query()
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        if (existingUser != null)
        {
            existingUser.FirstName = request.FirstName;
            existingUser.LastName = request.LastName;
            existingUser.UpdatedAt = DateTimeOffset.UtcNow;
            await _repository.UpdateAsync(existingUser, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return existingUser.Id;
        }

        var entity = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return entity.Id;
    }
}
