using MediatR;

namespace TimeTracker.Application.Users.Commands;

public record SyncUserCommand(string Email, string FirstName, string LastName) : IRequest<string>;
