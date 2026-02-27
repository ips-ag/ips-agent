# MCP Integration Specification

## 1. Overview

Extend the existing TimeTracker API with [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) endpoints, enabling AI agents to create time entries on behalf of authenticated users. The integration uses **Streamable HTTP transport** and exposes a minimal set of MCP tools that mirror existing HTTP functionality.

### Technology Choice

| Component | Choice | Rationale |
|-----------|--------|-----------|
| MCP SDK | [official `ModelContextProtocol` NuGet package](https://github.com/modelcontextprotocol/csharp-sdk) | Microsoft-backed, first-party .NET MCP SDK; supports ASP.NET integration via `MapMcp()` |

### Design Principles

- **Extension only** — no changes to existing controllers, middleware, or application layer code.
- **User identity** — all MCP tools execute under the authenticated user's identity via the existing `ICurrentUserService` and `UserSyncMiddleware`.
- **Minimal surface** — only tools required to look up context and create time entries are exposed.

---

## 2. Transport & Endpoint

| Setting | Value |
|---------|-------|
| Transport | Streamable HTTP |
| Route | `/mcp` |
| Auth | Same Entra ID JWT Bearer as HTTP API (`[Authorize]`) |

The MCP endpoint is mounted alongside the existing controller routes in `Program.cs`:

```csharp
app.MapMcp("/mcp").RequireAuthorization();
```

This ensures the existing authentication middleware (`UseAuthentication`, `UseAuthorization`) and `UserSyncMiddleware` process MCP requests identically to HTTP API requests.

---

## 3. MCP Tools

Six tools are exposed — five read-only lookups plus one write operation. Each tool reuses existing MediatR commands/queries, requiring no application layer changes.

### 3.1 `get_my_projects`

| Field | Value |
|-------|-------|
| **Description** | Returns all active projects assigned to the currently authenticated user. Use this to discover which projects the user can log time against. Each project includes its ID, name, code, customer name, and active date range. |
| **Parameters** | *(none)* |
| **Returns** | `List<ProjectDto>` — flat list of projects assigned to the current user |
| **MediatR Query** | `GetProjectsByUserQuery(userId)` |
| **Auth** | Required — uses caller's identity to resolve user ID |

### 3.2 `get_project_tasks`

| Field | Value |
|-------|-------|
| **Description** | Returns all active tasks under a specific project. Time entries are logged against tasks (not projects), so this tool is essential to resolve the correct `taskId` before creating a time entry. Each task includes its ID, name, code, description, and active date range. |
| **Parameters** | `projectId` (string, required) — the project ID obtained from `get_my_projects` |
| **Returns** | `List<TaskDto>` — tasks belonging to the specified project |
| **MediatR Query** | `GetTasksByProjectQuery(projectId)` |
| **Auth** | Required |

### 3.3 `get_my_timesheet`

| Field | Value |
|-------|-------|
| **Description** | Returns the authenticated user's timesheet for the week containing the specified date. Includes all time entries for that week with task/project names and a total hours sum. Use this to check existing entries before creating duplicates or to verify remaining capacity for a given day (max 24h/day). |
| **Parameters** | `date` (string, required, format `YYYY-MM-DD`) — any date |
| **Returns** | `TimesheetDto` — week start date, list of `TimeEntryDto`, total hours |
| **MediatR Query** | `GetMyTimesheetQuery(userId, date)` |
| **Auth** | Required — uses caller's identity to resolve user ID |

### 3.4 `get_my_time_entries`

| Field | Value |
|-------|-------|
| **Description** | Returns the authenticated user's time entries filtered by optional date range and/or task. Use this to check whether a specific entry already exists before creating a new one, or to review logged hours for a specific task or period. |
| **Parameters** | `taskId` (string, optional) — filter by task; `dateFrom` (string, optional, `YYYY-MM-DD`) — inclusive start; `dateTo` (string, optional, `YYYY-MM-DD`) — inclusive end |
| **Returns** | `PagedList<TimeEntryDto>` |
| **MediatR Query** | `GetTimeEntriesQuery(page: 1, pageSize: 100, userId, taskId, dateFrom, dateTo)` |
| **Auth** | Required — automatically scoped to the current user's ID |

### 3.5 `get_task_details`

| Field | Value |
|-------|-------|
| **Description** | Returns detailed information about a specific task, including its project association, description, code, and active status. Use this to confirm a task is active and valid before creating a time entry. |
| **Parameters** | `taskId` (string, required) — the task ID |
| **Returns** | `TaskDto` |
| **MediatR Query** | `GetTaskByIdQuery(taskId)` |
| **Auth** | Required |

### 3.6 `create_time_entry`

| Field | Value |
|-------|-------|
| **Description** | Creates a new time entry for the authenticated user. The entry is logged against a specific task on a specific date. Hours must be between 0.25 and 24.00 in 0.25 increments. The total hours for the user on the given date must not exceed 24. Returns the ID of the created time entry. |
| **Parameters** | `taskId` (string, required) — target task ID; `date` (string, required, `YYYY-MM-DD`) — the date the work was performed; `hours` (number, required) — hours worked (0.25–24.00, 0.25 increments); `description` (string, required, max 500) — description of work performed |
| **Returns** | `string` — the new time entry's ID |
| **MediatR Command** | `CreateTimeEntryCommand(userId, taskId, date, hours, description)` |
| **Auth** | Required — `userId` is resolved from the caller's identity, never accepted as input |

---

## 4. Implementation Details

### 4.1 NuGet Package

Add to `TimeTracker.csproj`:

```xml
<PackageReference Include="ModelContextProtocol.AspNetCore" Version="0.*" />
```

### 4.2 MCP Server Registration

In `Program.cs`, register the MCP server before `var app = builder.Build()`:

```csharp
builder.Services
    .AddMcpServer()
    .WithStreamableHttpTransport()
    .WithToolsFromAssembly();
```

After the existing middleware pipeline, map the MCP endpoint:

```csharp
app.MapMcp("/mcp").RequireAuthorization();
```

### 4.3 Tool Implementation Pattern

All tools are implemented in a single class using the `[McpServerTool]` attribute. Tools resolve `ISender` and `ICurrentUserService` from DI to dispatch existing MediatR requests.

```
src/api/TimeTracker/
└── Mcp/
    └── TimeTrackerMcpTools.cs
```

Example tool skeleton:

```csharp
[McpServerToolType]
public sealed class TimeTrackerMcpTools
{
    [McpServerTool(Name = "get_my_projects"),
     Description("Returns all active projects assigned to the current user...")]
    public async Task<List<ProjectDto>> GetMyProjects(
        ISender sender, ICurrentUserService currentUser, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();
        return await sender.Send(new GetProjectsByUserQuery(userId), ct);
    }

    [McpServerTool(Name = "create_time_entry"),
     Description("Creates a time entry for the current user...")]
    public async Task<string> CreateTimeEntry(
        ISender sender, ICurrentUserService currentUser,
        string taskId, string date, decimal hours, string description,
        CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();
        var command = new CreateTimeEntryCommand(userId, taskId,
            DateOnly.Parse(date), hours, description);
        return await sender.Send(command, ct);
    }
}
```

### 4.4 Authentication Flow

```
Agent → HTTP POST /mcp (Bearer token) → ASP.NET Auth Middleware → UserSyncMiddleware → MCP Handler → MediatR
```

The connecting agent must acquire an Entra ID token with the `api://<client-id>/FakeIntra` scope — the same OAuth2 flow used by the SPA. The MCP SDK's Streamable HTTP transport receives standard HTTP requests, so the existing middleware pipeline processes them without modification.

### 4.5 Error Handling

MCP tool exceptions are surfaced as MCP error responses. The existing `ValidationBehaviour` pipeline in MediatR will throw `ValidationException` for invalid inputs (e.g., hours out of range). These propagate as MCP tool errors with descriptive messages — no additional error mapping is needed.

---

## 5. Example Workflow

**User prompt:** _"I worked for 4 hours on the Acme Redesign project today."_

The agent has no prior context (project ID, task ID, or existing entries). Today's date is resolved from the agent's system context.

### Step 1 — Discover the user's projects

```
tool: get_my_projects
params: (none)
```

Response contains a list of `ProjectDto`. The agent searches the list for a name matching **"Acme Redesign"** (fuzzy or exact). Match found:

```json
{ "id": "proj-abc123", "name": "Acme Redesign", "code": "ACR", "customerName": "Acme Corp", "isActive": true }
```

### Step 2 — Retrieve tasks for that project

```
tool: get_project_tasks
params: { "projectId": "proj-abc123" }
```

Response lists active tasks. If there is exactly one active task, the agent selects it automatically. If multiple tasks exist, the agent asks the user to clarify which task the work relates to before continuing.

Assume one task is returned:

```json
{ "id": "task-xyz789", "name": "Frontend Development", "code": "ACR-FE", "isActive": true }
```

### Step 3 — Check existing time for today

```
tool: get_my_time_entries
params: { "dateFrom": "2026-02-27", "dateTo": "2026-02-27" }
```

The agent sums `hours` across all returned entries. If the total plus the requested 4 hours would exceed 24, it informs the user and stops. Here the day is empty (0 h logged), so it is safe to proceed.

### Step 4 — Create the time entry

```
tool: create_time_entry
params: {
  "taskId":      "task-xyz789",
  "date":        "2026-02-27",
  "hours":       4,
  "description": "Worked on Acme Redesign – Frontend Development"
}
```

Response: the new time entry ID, e.g. `"te-0011ef"`.

The agent confirms to the user: _"Logged 4 hours on **Acme Redesign / Frontend Development** for today (27 Feb 2026)."_

---

### Decision points

| Situation | Agent behaviour |
|-----------|----------------|
| No project name match | Ask user to clarify; offer list of their active projects |
| Multiple projects partially match | Present matched names and ask user to confirm |
| Multiple tasks available on the project | Ask user which task the work should be logged against |
| Day capacity would be exceeded | Inform user of remaining capacity; do not create the entry |
