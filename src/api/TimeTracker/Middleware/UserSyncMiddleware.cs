using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Middleware;

public class UserSyncMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserSyncMiddleware> _logger;

    public UserSyncMiddleware(RequestDelegate next, ILogger<UserSyncMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IRepository<User> userRepo, IUnitOfWork uow)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            string? sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.User.FindFirstValue("sub");
            if (sub is not null)
            {
                var existing = await userRepo.Query().FirstOrDefaultAsync(u => u.ExternalId == sub);

                if (existing is null)
                {
                    string email = context.User.FindFirstValue(ClaimTypes.Email) ??
                        context.User.FindFirstValue("preferred_username") ?? string.Empty;
                    var user = new User
                    {
                        Id = Guid.NewGuid().ToString(),
                        ExternalId = sub,
                        Email = email,
                        FirstName =
                            context.User.FindFirstValue(ClaimTypes.GivenName) ??
                            context.User.FindFirstValue("given_name") ?? string.Empty,
                        LastName = context.User.FindFirstValue(ClaimTypes.Surname) ??
                            context.User.FindFirstValue("family_name") ?? string.Empty
                    };
                    await userRepo.AddAsync(user);
                    await uow.SaveChangesAsync();

                    _logger.LogInformation("Auto-provisioned new user {Email} (sub: {Sub})", email, sub);
                }
            }
        }
        await _next(context);
    }
}
