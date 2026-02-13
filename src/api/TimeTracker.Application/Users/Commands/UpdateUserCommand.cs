using MediatR;

namespace TimeTracker.Application.Users.Commands;

public record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive) : IRequest<MediatR.Unit>;
