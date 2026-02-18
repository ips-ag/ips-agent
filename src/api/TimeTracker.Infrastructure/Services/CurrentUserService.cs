using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TimeTracker.Application.Common.Interfaces;

namespace TimeTracker.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var oid = User?.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
                      ?? User?.FindFirstValue("oid");
            return oid is not null ? Guid.Parse(oid) : null;
        }
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email)
                            ?? User?.FindFirstValue("preferred_username");

    public string? Role => User?.FindFirstValue(ClaimTypes.Role);
}
