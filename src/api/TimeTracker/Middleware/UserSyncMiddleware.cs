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
            var oid = context.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
                      ?? context.User.FindFirstValue("oid");

            if (oid is not null)
            {
                var existing = await userRepo.Query()
                    .FirstOrDefaultAsync(u => u.ExternalId == oid);

                if (existing is null)
                {
                    var email = context.User.FindFirstValue(ClaimTypes.Email)
                                ?? context.User.FindFirstValue("preferred_username")
                                ?? string.Empty;

                    var user = new User
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = oid,
                        Email = email,
                        FirstName = context.User.FindFirstValue("given_name") ?? string.Empty,
                        LastName = context.User.FindFirstValue("family_name") ?? string.Empty,
                    };

                    await userRepo.AddAsync(user);
                    await uow.SaveChangesAsync();

                    _logger.LogInformation("Auto-provisioned new user {Email} (oid: {Oid})", email, oid);
                }
            }
        }

        await _next(context);
    }
}
