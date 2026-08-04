using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Users.Queries;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IRepository<User> _repository;

    public GetUserByIdQueryHandler(IRepository<User> repository)
    {
        _repository = repository;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var entity = await _repository.Query()
            .FirstOrDefaultAsync(u => u.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(User), request.Id);

        return UserDto.FromEntity(entity);
    }
}
