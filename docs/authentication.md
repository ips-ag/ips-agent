# Authentication Implementation Plan

## Overview

Add Microsoft Entra ID (Azure AD) authentication to the Fake Intra application. The Entra ID app registration is already completed (`app-fakeintra-dev`). Both `Microsoft.Identity.Web` (API) and `@azure/msal-browser` + `@azure/msal-react` (SPA) packages are already installed but not wired up.

### Entra ID Configuration Reference

| Setting | Value |
|---------|-------|
| Tenant ID | `dcb58767-2d57-462f-82d5-552df1c47ccb` |
| Client ID | `17e121f3-96fa-43fc-948f-75d9798cead4` |
| App ID URI | `api://17e121f3-96fa-43fc-948f-75d9798cead4` |
| Authority | `https://login.microsoftonline.com/dcb58767-2d57-462f-82d5-552df1c47ccb` |
| API Scope | `api://17e121f3-96fa-43fc-948f-75d9798cead4/FakeIntra` |
| SPA Redirect (dev) | `http://localhost:5173/` |
| Optional Claims | `email`, `given_name`, `family_name` |

---

## Current State

| Component | Status |
|-----------|--------|
| Entra ID app registration | Done |
| `Microsoft.Identity.Web` NuGet package | Installed (not configured) |
| `@azure/msal-browser` + `@azure/msal-react` npm packages | Installed (not configured) |
| `ICurrentUserService` interface | Defined (no implementation) |
| Axios interceptor | Reads token from `sessionStorage` (no MSAL integration) |
| `appsettings.Development.json` AzureAd section | Placeholder values |
| Controllers `[Authorize]` | Missing |
| `User.ExternalId` field for Entra ID mapping | Missing |
| MSAL Provider in React tree | Missing |
| Protected routes | Missing |
| TopBar sign-out | Placeholder |

---

## Implementation Steps

### Phase 1 — API Authentication

#### Step 1.1: Update `appsettings.Development.json`

Replace placeholder values in the `AzureAd` section with actual Entra ID values:

```jsonc
// src/api/TimeTracker/appsettings.Development.json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "dcb58767-2d57-462f-82d5-552df1c47ccb",
    "ClientId": "17e121f3-96fa-43fc-948f-75d9798cead4",
    "Audience": "api://17e121f3-96fa-43fc-948f-75d9798cead4"
  }
}
```

#### Step 1.2: Add `AzureAd` section to `appsettings.json`

Add a production-ready section with empty/overridable values (populated via environment variables or Azure App Configuration in deployed environments):

```jsonc
// src/api/TimeTracker/appsettings.json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "",
    "ClientId": "",
    "Audience": ""
  }
}
```

#### Step 1.3: Configure authentication in `Program.cs`

Wire up JWT Bearer authentication using `Microsoft.Identity.Web`:

```csharp
// src/api/TimeTracker/Program.cs
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add authentication
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// ... existing service registrations ...

var app = builder.Build();

// ... existing middleware (Swagger, ExceptionHandling, CORS) ...

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
```

#### Step 1.4: Configure Swagger for OAuth2

Add Swagger OAuth2 support so developers can test authenticated endpoints from the Swagger UI:

```csharp
// src/api/TimeTracker/Program.cs — inside AddSwaggerGen
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://login.microsoftonline.com/dcb58767-2d57-462f-82d5-552df1c47ccb/oauth2/v2.0/authorize"),
                TokenUrl = new Uri("https://login.microsoftonline.com/dcb58767-2d57-462f-82d5-552df1c47ccb/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "api://17e121f3-96fa-43fc-948f-75d9798cead4/FakeIntra", "Access FakeIntra API" }
                }
            }
        }
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            new[] { "api://17e121f3-96fa-43fc-948f-75d9798cead4/FakeIntra" }
        }
    });
});

// In the SwaggerUI configuration:
app.UseSwaggerUI(options =>
{
    options.OAuthClientId("17e121f3-96fa-43fc-948f-75d9798cead4");
    options.OAuthUsePkce();
});
```

#### Step 1.5: Add `[Authorize]` to all controllers

Apply `[Authorize]` attribute to every controller class. All API endpoints require authentication by default:

- `CustomersController`
- `ProjectsController`
- `ReportsController`
- `TasksController`
- `TimeEntriesController`
- `TimesheetsController`
- `UnitsController`
- `UsersController`

```csharp
using Microsoft.AspNetCore.Authorization;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase { ... }
```

#### Step 1.6: Add `ExternalId` to `User` entity

Add an `ExternalId` property to map Entra ID object IDs to local users:

```csharp
// src/api/TimeTracker.Domain/Entities/User.cs
public class User
{
    public Guid Id { get; init; }
    public string ExternalId { get; set; } = string.Empty;  // Entra ID oid claim
    public string Email { get; set; } = string.Empty;
    // ... rest of properties
}
```

Add this column in a new EF Core migration.

#### Step 1.7: Implement `ICurrentUserService`

Create a `CurrentUserService` that extracts user identity from the JWT claims in `HttpContext`:

```csharp
// src/api/TimeTracker.Infrastructure/Services/CurrentUserService.cs
using System.Security.Claims;
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
```

#### Step 1.8: Register `ICurrentUserService` in DI

```csharp
// src/api/TimeTracker/Program.cs (or Infrastructure DependencyInjection.cs)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
```

#### Step 1.9: Create EF Core migration for `ExternalId`

```bash
dotnet ef migrations add AddUserExternalId --project src/api/TimeTracker.Infrastructure --startup-project src/api/TimeTracker
```

---

### Phase 2 — SPA Authentication

#### Step 2.1: Create MSAL configuration

```typescript
// src/app/src/auth/msalConfig.ts
import { Configuration, LogLevel } from '@azure/msal-browser';

export const msalConfig: Configuration = {
  auth: {
    clientId: '17e121f3-96fa-43fc-948f-75d9798cead4',
    authority: 'https://login.microsoftonline.com/dcb58767-2d57-462f-82d5-552df1c47ccb',
    redirectUri: '/',
    postLogoutRedirectUri: '/',
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      logLevel: LogLevel.Warning,
    },
  },
};

export const loginRequest = {
  scopes: ['openid', 'profile', 'email', 'api://17e121f3-96fa-43fc-948f-75d9798cead4/FakeIntra'],
};

export const apiScopes = {
  scopes: ['api://17e121f3-96fa-43fc-948f-75d9798cead4/FakeIntra'],
};
```

#### Step 2.2: Wrap app with `MsalProvider`

```tsx
// src/app/src/main.tsx
import { PublicClientApplication } from '@azure/msal-browser';
import { MsalProvider } from '@azure/msal-react';
import { msalConfig } from './auth/msalConfig';

const msalInstance = new PublicClientApplication(msalConfig);

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <MsalProvider instance={msalInstance}>
      <BrowserRouter>
        <QueryClientProvider client={queryClient}>
          <ThemeProvider theme={theme}>
            <CssBaseline />
            <App />
          </ThemeProvider>
        </QueryClientProvider>
      </BrowserRouter>
    </MsalProvider>
  </React.StrictMode>,
);
```

#### Step 2.3: Create `useAuth` hook

Create a custom hook that wraps MSAL's `useMsal` and provides convenient auth operations:

```typescript
// src/app/src/hooks/useAuth.ts
import { useMsal, useAccount } from '@azure/msal-react';
import { loginRequest, apiScopes } from '../auth/msalConfig';

export function useAuth() {
  const { instance, accounts } = useMsal();
  const account = useAccount(accounts[0] ?? {});

  const login = () => instance.loginRedirect(loginRequest);
  const logout = () => instance.logoutRedirect();

  const getAccessToken = async (): Promise<string> => {
    if (!account) throw new Error('No account');
    const response = await instance.acquireTokenSilent({
      ...apiScopes,
      account,
    });
    return response.accessToken;
  };

  return {
    isAuthenticated: accounts.length > 0,
    user: account
      ? {
          name: account.name ?? '',
          email: account.username ?? '',
          givenName: account.idTokenClaims?.given_name as string | undefined,
          familyName: account.idTokenClaims?.family_name as string | undefined,
        }
      : null,
    login,
    logout,
    getAccessToken,
  };
}
```

#### Step 2.4: Update Axios interceptor to use MSAL

Replace the `sessionStorage` token read with MSAL's `acquireTokenSilent`:

```typescript
// src/app/src/api/client.ts
import { PublicClientApplication, InteractionRequiredAuthError } from '@azure/msal-browser';
import { msalConfig, apiScopes } from '../auth/msalConfig';

const msalInstance = new PublicClientApplication(msalConfig);

apiClient.interceptors.request.use(async (config) => {
  const accounts = msalInstance.getAllAccounts();
  if (accounts.length > 0) {
    try {
      const response = await msalInstance.acquireTokenSilent({
        ...apiScopes,
        account: accounts[0],
      });
      config.headers.Authorization = `Bearer ${response.accessToken}`;
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        await msalInstance.acquireTokenRedirect(apiScopes);
      }
    }
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      await msalInstance.loginRedirect();
    }
    return Promise.reject(error);
  },
);
```

#### Step 2.5: Create `ProtectedRoute` component

Wrap authenticated routes so unauthenticated users are redirected to login:

```tsx
// src/app/src/components/auth/ProtectedRoute.tsx
import { useIsAuthenticated, useMsal } from '@azure/msal-react';
import { loginRequest } from '../../auth/msalConfig';

export default function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useIsAuthenticated();
  const { instance, inProgress } = useMsal();

  if (inProgress !== 'none') {
    return <CircularProgress />; // or a loading skeleton
  }

  if (!isAuthenticated) {
    instance.loginRedirect(loginRequest);
    return null;
  }

  return <>{children}</>;
}
```

#### Step 2.6: Protect all routes in `App.tsx`

Wrap the entire `AppShell` (or individual routes) in `ProtectedRoute`:

```tsx
// src/app/src/App.tsx
import ProtectedRoute from './components/auth/ProtectedRoute';

export default function App() {
  return (
    <ProtectedRoute>
      <AppShell>
        <Routes>
          {/* ... all existing routes ... */}
        </Routes>
      </AppShell>
    </ProtectedRoute>
  );
}
```

#### Step 2.7: Update `TopBar` with real user info & sign-out

Wire the TopBar avatar and sign-out button to MSAL:

```tsx
// src/app/src/components/layout/TopBar.tsx
import { useAuth } from '../../hooks/useAuth';

// Inside TopBar component:
const { user, logout } = useAuth();

// Avatar: show user initials
<Avatar>{user?.givenName?.[0]}{user?.familyName?.[0]}</Avatar>

// Sign Out onClick:
<MenuItem onClick={() => logout()}>Sign Out</MenuItem>
```

#### Step 2.8: Update `ProfilePage` with real user info

Replace hardcoded user data with claims from MSAL:

```tsx
// src/app/src/pages/ProfilePage.tsx
import { useAuth } from '../hooks/useAuth';

const { user } = useAuth();
// Display user.name, user.email, user.givenName, user.familyName
```

---

### Phase 3 — User Sync & Authorization

#### Step 3.1: Auto-sync user on first login

When a user authenticates for the first time, sync their Entra ID profile to the local `User` table. This can be done via the existing `SyncUserCommand` on the `POST /api/v1/users` endpoint, called from the SPA after login, or via a middleware/filter on the API side that checks if the `oid` claim maps to an existing user and creates one if not.

**Recommended approach — API-side middleware:**

```csharp
// src/api/TimeTracker/Middleware/UserSyncMiddleware.cs
public class UserSyncMiddleware
{
    public async Task InvokeAsync(HttpContext context, IRepository<User> userRepo, IUnitOfWork uow)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var oid = context.User.FindFirstValue("oid");
            var existing = await userRepo.Query()
                .FirstOrDefaultAsync(u => u.ExternalId == oid);

            if (existing is null)
            {
                var user = new User
                {
                    ExternalId = oid,
                    Email = context.User.FindFirstValue("email"),
                    FirstName = context.User.FindFirstValue("given_name") ?? "",
                    LastName = context.User.FindFirstValue("family_name") ?? "",
                };
                await userRepo.AddAsync(user);
                await uow.SaveChangesAsync();
            }
        }
        await _next(context);
    }
}
```

#### Step 3.2: Role-based authorization (optional enhancement)

Map Entra ID roles or groups to the application's `UserRole` enum (`Admin`, `Manager`, `Employee`) and enforce role-based policies:

```csharp
// Example: restrict admin endpoints
[Authorize(Roles = "Admin")]
[HttpDelete("{id:guid}")]
public async Task<IActionResult> Deactivate(Guid id, ...) { ... }
```

This requires configuring App Roles in the Entra ID app registration and mapping them to the token's `roles` claim.

---

## File Change Summary

| File | Action | Description |
|------|--------|-------------|
| `src/api/TimeTracker/appsettings.json` | Modify | Add `AzureAd` section |
| `src/api/TimeTracker/appsettings.Development.json` | Modify | Replace placeholder values with real Entra ID config |
| `src/api/TimeTracker/Program.cs` | Modify | Add `AddMicrosoftIdentityWebApiAuthentication`, `UseAuthentication`, `UseAuthorization`, Swagger OAuth2 |
| `src/api/TimeTracker/Controllers/*.cs` | Modify | Add `[Authorize]` to all 8 controllers |
| `src/api/TimeTracker.Domain/Entities/User.cs` | Modify | Add `ExternalId` property |
| `src/api/TimeTracker.Infrastructure/Services/CurrentUserService.cs` | Create | Implement `ICurrentUserService` from JWT claims |
| `src/api/TimeTracker.Infrastructure/DependencyInjection.cs` | Modify | Register `ICurrentUserService` |
| `src/api/TimeTracker.Infrastructure/Migrations/` | Create | New migration for `ExternalId` column |
| `src/app/src/auth/msalConfig.ts` | Create | MSAL configuration & scope definitions |
| `src/app/src/main.tsx` | Modify | Wrap app with `MsalProvider` |
| `src/app/src/hooks/useAuth.ts` | Create | Custom auth hook wrapping MSAL |
| `src/app/src/api/client.ts` | Modify | Replace sessionStorage token with MSAL `acquireTokenSilent` |
| `src/app/src/components/auth/ProtectedRoute.tsx` | Create | Auth guard component |
| `src/app/src/App.tsx` | Modify | Wrap routes in `ProtectedRoute` |
| `src/app/src/components/layout/TopBar.tsx` | Modify | Wire avatar & sign-out to MSAL |
| `src/app/src/pages/ProfilePage.tsx` | Modify | Display real user info from token claims |

## Implementation Order

```
Phase 1 (API):   1.1 → 1.2 → 1.3 → 1.4 → 1.5 → 1.6 → 1.7 → 1.8 → 1.9
Phase 2 (SPA):   2.1 → 2.2 → 2.3 → 2.4 → 2.5 → 2.6 → 2.7 → 2.8
Phase 3 (Sync):  3.1 → 3.2
```

Phases 1 and 2 can be worked on in parallel. Phase 3 depends on both being complete.

## Verification Checklist

- [ ] API returns `401 Unauthorized` when called without a token
- [ ] API returns `200 OK` when called with a valid Entra ID Bearer token
- [ ] Swagger UI shows the "Authorize" button and completes the OAuth2 flow
- [ ] SPA redirects to Entra ID login when unauthenticated
- [ ] SPA acquires tokens silently after initial login
- [ ] API calls from SPA include `Authorization: Bearer <token>` header
- [ ] TopBar shows authenticated user's initials and name
- [ ] Sign Out clears the session and redirects to Entra ID logout
- [ ] Profile page shows the logged-in user's info
- [ ] First-time users are auto-created in the local database
