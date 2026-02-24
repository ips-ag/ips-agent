using MediatR;

namespace TimeTracker.Application.Users.Commands;

public record DeactivateUserCommand(string Id) : IRequest<MediatR.Unit>;
