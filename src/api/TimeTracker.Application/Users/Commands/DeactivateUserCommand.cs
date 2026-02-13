using MediatR;

namespace TimeTracker.Application.Users.Commands;

public record DeactivateUserCommand(Guid Id) : IRequest<MediatR.Unit>;
