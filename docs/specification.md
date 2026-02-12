# Fake Intra — Implementation Plan

## 1. Overview

**Fake Intra** is a web application for tracking hours and reporting project work within an organization. Employees log time entries against projects, and managers/admins generate reports for billing, capacity planning, and productivity analysis.

| Layer | Technology | Version |
|-------|-----------|---------|
| Frontend | React | 19.x |
| Build tooling | Vite | 6.x |
| UI library | Material UI (MUI) | 6.x |
| Backend API | ASP.NET | 10.0 |
| CQRS / Mediator | MediatR | 12.5.0 (last OSS release) |
| ORM | Entity Framework Core | 10.0 |
| Database (prod) | SQL Server | 2025 |
| Database (dev) | SQLite | via EF Core provider |
| Auth | ASP.NET Identity + Entra ID + JWT | — |
| Testing | xUnit, Moq, React Testing Library | — |

### Design Colours

| Token | Hex | Usage |
|-------|-----|-------|
| Primary | `#17428c` | App bar, buttons, links, active states |
| Secondary | `#009dc3` | Accents, FABs, secondary buttons, highlights |
| Background | `#ffffff` | Page background, card surfaces |

---

## 2. Architecture

### 2.1 Clean Architecture Layers

```
┌──────────────────────────────────────────────────┐
│                   API Layer                      │
│          (ASP.NET Controllers, DTOs)             │
├──────────────────────────────────────────────────┤
│              Application Layer                   │
│   (MediatR Handlers, Commands, Queries,          │
│    Validators, Mapping Profiles)                 │
├──────────────────────────────────────────────────┤
│               Domain Layer                       │
│   (Entities, Value Objects, Enums,               │
│    Domain Events, Repository Interfaces)         │
├──────────────────────────────────────────────────┤
│           Infrastructure Layer                   │
│   (EF Core DbContext, Repositories,              │
│    Migrations, External Services)                │
└──────────────────────────────────────────────────┘
```

**Dependency rule:** Each layer depends only on the layer directly below it. Domain has zero external dependencies.

### 2.2 Solution Structure

```
ips-agent/
├── docs/
│   └── specification.md
├── src/
│   ├── TimeTracker.sln                      # Solution file with all api, app, and test projects
│   ├── api/
│   │   ├── TimeTracker.Domain/              # .NET 10 class library
│   │   ├── TimeTracker.Application/         # .NET 10 class library
│   │   ├── TimeTracker.Infrastructure/      # .NET 10 class library
│   │   └── TimeTracker/                     # ASP.NET 10 Web API
│   ├── app/                                 # React 19 SPA (Vite)
│   ├── tests/
│   │   └── TimeTracker.Tests/
└── README.md
```

---

## 3. Domain Model

### 3.1 Entities

```
┌──────────────┐
│     Unit     │
└──────┬───────┘
       │1
       │
       *
┌──────┴───────┐
│   Customer   │
└──────┬───────┘
       │1                 ┌──────────────┐
       │                  │     User     │──────┐
       *                  └──────┬───────┘      │
┌──────┴───────┐                 │*             │
│   Project    │◄─┐              │              │
└──────┬───────┘  │              │              │
       │1         │ ParentProject               │
       │          │ (self-reference)            │
       *          │               *             │1
       │          *       ┌───────┴────────┐    │
       │  ┌───────┴──────┐│ TaskUser       │    │
       │  │   Project    ││ (join table)   │    │
       │  │  (children)  │└────────────────┘    │
       │  └──────────────┘         │            │
       │1                          │*           │
       │                           │            │
       *                           │            │
┌──────┴───────┐                   │            │
│     Task     │                   │            │
└──────┬───────┘                   │            │
       │1                          │            │
       │                           │            │
       │                           │            *
       └───────────────────────────┘    ┌───────┴───────┐
                                        │   TimeEntry   │
                                        └───────────────┘
```

#### Unit

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | PK |
| Name | string | Required, max 200 |
| Description | string | Optional |
| IsActive | bool | Default: true |
| CreatedAt | DateTimeOffset | Auto |
| UpdatedAt | DateTimeOffset | Auto |

#### Customer

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | PK |
| UnitId | Guid | FK |
| Name | string | Required, max 200 |
| Description | string | Optional |
| ContactEmail | string | Optional, max 200 |
| ContactPhone | string | Optional, max 50 |
| IsActive | bool | Default: true |
| CreatedAt | DateTimeOffset | Auto |
| UpdatedAt | DateTimeOffset | Auto |

#### User

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | PK |
| Email | string | Unique, required |
| FirstName | string | Required, max 100 |
| LastName | string | Required, max 100 |
| Role | enum(Admin, Manager, Employee) | Default: Employee |
| IsActive | bool | Default: true |
| CreatedAt | DateTimeOffset | Auto |
| UpdatedAt | DateTimeOffset | Auto |

**Navigation Properties:**
- `ProjectUsers` — Collection of project assignments
- `TaskUsers` — Collection of task assignments
- `TimeEntries` — Collection of time entries logged by this user

#### Project

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | PK |
| CustomerId | Guid | FK |
| ParentId | Guid? | FK (nullable, for nested projects) |
| Name | string | Required, max 200 |
| Code | string | Required, max 100 |
| Description | string | Optional |
| IsActive | bool | Default: true |
| StartDate | DateOnly | Required |
| EndDate | DateOnly? | Nullable |
| CreatedAt | DateTimeOffset | Auto |
| UpdatedAt | DateTimeOffset | Auto |

**Hierarchy Rules:**
- Root projects: `ParentId` is `null`, must have `CustomerId`
- Child projects: `ParentId` references parent, inherits `CustomerId` from root ancestor

#### Task

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | PK |
| ProjectId | Guid | FK (parent project) |
| Name | string | Required, max 200 |
| Code | string | Required, max 100 |
| Description | string | Optional |
| IsActive | bool | Default: true |
| StartDate | DateOnly? | Nullable |
| EndDate | DateOnly? | Nullable |
| CreatedAt | DateTimeOffset | Auto |
| UpdatedAt | DateTimeOffset | Auto |

**Navigation Properties:**
- `TimeEntries` — Collection of time entries logged to this task

#### ProjectUser (Join Table)

| Property | Type | Notes |
|----------|------|-------|
| UserId | Guid | PK, FK |
| ProjectId | Guid | PK, FK |
| AssignedAt | DateTimeOffset | Auto |
| AssignedBy | Guid? | FK to User (nullable) |

**Composite Primary Key:** (`UserId`, `ProjectId`)

#### TaskUser (Join Table)

| Property | Type | Notes |
|----------|------|-------|
| UserId | Guid | PK, FK |
| TaskId | Guid | PK, FK |
| AssignedAt | DateTimeOffset | Auto |
| AssignedBy | Guid? | FK to User (nullable) |

**Composite Primary Key:** (`UserId`, `TaskId`)

#### TimeEntry

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | PK |
| UserId | Guid | FK |
| TaskId | Guid | FK |
| Date | DateOnly | Required |
| Hours | decimal(5,2) | Required, 0.25–24.00 |
| Description | string | Required, max 500 |
| CreatedAt | DateTimeOffset | Auto |
| UpdatedAt | DateTimeOffset | Auto |

### 3.2 Value Objects / Enums

```csharp
public enum UserRole { Admin, Manager, Employee }
```

### 3.3 Domain Rules

1. A time entry's `Hours` must be between 0.25 and 24.00 (in 0.25 increments).
2. A user cannot log more than 24 hours in a single day across all tasks.
3. Users can edit or delete their own time entries.
4. Managers and Admins can view and manage all time entries.
5. **Unit → Customer:** Each Customer must belong to exactly one Unit.
6. **Customer → Project:** Root projects must belong to exactly one Customer.
7. **Project Nesting:** Projects can be nested to any depth via `ParentId`.
8. **Project → Task:** Each Task must belong to exactly one Project (any level).
9. **Time Entry Rules:** Time can ONLY be logged against Tasks, not Projects.
10. **User Assignment:** Users can be assigned to Projects and/or Tasks; assignment is optional for time tracking.
11. **Cascade Rules:** Deleting a Unit soft-deletes Customers; deleting a Customer soft-deletes root Projects; deleting a Project soft-deletes child Projects and Tasks; deleting a User removes project and task assignments.
12. **Archiving vs Deletion:** Units, Customers, Projects, and Tasks are archived (soft-deleted) to preserve history; TimeEntries are hard-deleted when removed.
13. **IsActive** If `IsActive` is `false`, the entity is considered inactive and should be excluded from active queries and cannot be used for new time entries or assignments.

---

## 4. Application Layer — CQRS with MediatR

All use-cases are modelled as **Commands** (write) or **Queries** (read) using MediatR 12.5.0.

### 4.1 Commands

**Note:** Commands that create entities return `Guid` (the new entity's ID). Commands that update or delete return `Unit` (MediatR's void-equivalent type, not to be confused with the Unit entity).

| Command | Handler produces |
|---------|-----------------|
| `CreateUnitCommand` | Guid |
| `UpdateUnitCommand` | Unit |
| `ArchiveUnitCommand` | Unit |
| `CreateCustomerCommand` | Guid |
| `UpdateCustomerCommand` | Unit |
| `ArchiveCustomerCommand` | Unit |
| `CreateProjectCommand` | Guid |
| `UpdateProjectCommand` | Unit |
| `ArchiveProjectCommand` | Unit |
| `CreateTaskCommand` | Guid |
| `UpdateTaskCommand` | Unit |
| `ArchiveTaskCommand` | Unit |
| `SyncUserCommand` | Guid |
| `UpdateUserCommand` | Unit |
| `DeactivateUserCommand` | Unit |
| `CreateTimeEntryCommand` | Guid |
| `UpdateTimeEntryCommand` | Unit |
| `DeleteTimeEntryCommand` | Unit |
| `AssignUserToProjectCommand` | Unit |
| `RemoveUserFromProjectCommand` | Unit |
| `BulkAssignUsersToProjectCommand` | Unit |
| `AssignUserToTaskCommand` | Unit |
| `RemoveUserFromTaskCommand` | Unit |
| `BulkAssignUsersToTaskCommand` | Unit |

### 4.2 Queries

| Query | Returns |
|-------|---------|
| `GetUnitsQuery` | PagedList\<UnitDto\> |
| `GetUnitByIdQuery` | UnitDto |
| `GetCustomersQuery` | PagedList\<CustomerDto\> |
| `GetCustomersByUnitQuery` | List\<CustomerDto\> |
| `GetCustomerByIdQuery` | CustomerDto |
| `GetProjectsQuery` | PagedList\<ProjectDto\> |
| `GetProjectsByCustomerQuery` | List\<ProjectDto\> (root projects only) |
| `GetProjectHierarchyQuery` | ProjectTreeDto (full tree) |
| `GetProjectByIdQuery` | ProjectDto |
| `GetChildProjectsQuery` | List\<ProjectDto\> |
| `GetTasksQuery` | PagedList\<TaskDto\> |
| `GetTasksByProjectQuery` | List\<TaskDto\> |
| `GetTaskByIdQuery` | TaskDto |
| `GetUsersQuery` | PagedList\<UserDto\> |
| `GetUserByIdQuery` | UserDto |
| `GetUsersByProjectQuery` | List\<UserDto\> |
| `GetProjectsByUserQuery` | List\<ProjectDto\> |
| `GetUsersByTaskQuery` | List\<UserDto\> |
| `GetTasksByUserQuery` | List\<TaskDto\> |
| `GetTimeEntriesQuery` | PagedList\<TimeEntryDto\> |
| `GetMyTimesheetQuery` | TimesheetDto (week view) |
| `GetProjectReportQuery` | ProjectReportDto |
| `GetUserReportQuery` | UserReportDto |
| `GetOverallReportQuery` | OverallReportDto |

### 4.3 Validation

Use **FluentValidation** integrated via MediatR pipeline behaviour:

```csharp
public class ValidationBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
```

### 4.4 Cross-Cutting Pipeline Behaviours

| Behaviour | Purpose |
|-----------|---------|
| `ValidationBehaviour` | Runs FluentValidation validators before handler |
| `LoggingBehaviour` | Structured logging of request/response |
| `UnhandledExceptionBehaviour` | Global exception → ProblemDetails mapping |

---

## 5. Infrastructure Layer

### 5.1 Entity Framework Core

#### DbContext

```csharp
public class TimeTrackerDbContext : DbContext
{
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Task> Tasks => Set<Task>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ProjectUser> ProjectUsers => Set<ProjectUser>();
    public DbSet<TaskUser> TaskUsers => Set<TaskUser>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
}
```

#### Entity Configuration Notes

```csharp
// Project self-referencing relationship
modelBuilder.Entity<Project>()
    .HasOne(p => p.ParentProject)
    .WithMany(p => p.ChildProjects)
    .HasForeignKey(p => p.ParentId)
    .OnDelete(DeleteBehavior.Restrict);  // Prevent accidental cascade

// Project -> Customer relationship
modelBuilder.Entity<Project>()
    .HasOne(p => p.Customer)
    .WithMany(c => c.Projects)
    .HasForeignKey(p => p.CustomerId)
    .OnDelete(DeleteBehavior.Restrict);

// User <-> Project many-to-many via ProjectUser
modelBuilder.Entity<ProjectUser>()
    .HasKey(pu => new { pu.UserId, pu.ProjectId });

modelBuilder.Entity<ProjectUser>()
    .HasOne(pu => pu.User)
    .WithMany(u => u.ProjectUsers)
    .HasForeignKey(pu => pu.UserId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<ProjectUser>()
    .HasOne(pu => pu.Project)
    .WithMany(p => p.ProjectUsers)
    .HasForeignKey(pu => pu.ProjectId)
    .OnDelete(DeleteBehavior.Cascade);

// User <-> Task many-to-many via TaskUser
modelBuilder.Entity<TaskUser>()
    .HasKey(tu => new { tu.UserId, tu.TaskId });

modelBuilder.Entity<TaskUser>()
    .HasOne(tu => tu.User)
    .WithMany(u => u.TaskUsers)
    .HasForeignKey(tu => tu.UserId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<TaskUser>()
    .HasOne(tu => tu.Task)
    .WithMany(t => t.TaskUsers)
    .HasForeignKey(tu => tu.TaskId)
    .OnDelete(DeleteBehavior.Cascade);

// Task -> Project relationship
modelBuilder.Entity<Task>()
    .HasOne(t => t.Project)
    .WithMany(p => p.Tasks)
    .HasForeignKey(t => t.ProjectId)
    .OnDelete(DeleteBehavior.Cascade);

// TimeEntry -> Task relationship
modelBuilder.Entity<TimeEntry>()
    .HasOne(te => te.Task)
    .WithMany(t => t.TimeEntries)
    .HasForeignKey(te => te.TaskId)
    .OnDelete(DeleteBehavior.Restrict);
```

#### Database Provider Selection (dev vs prod)

```csharp
// Program.cs
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<TimeTrackerDbContext>(options =>
        options.UseSqlite(connectionString));
}
else
{
    builder.Services.AddDbContext<TimeTrackerDbContext>(options =>
        options.UseSqlServer(connectionString));
}
```

#### Migrations Strategy

- Maintain **two migration sets** (SQLite for dev, SQL Server for prod) using EF Core's provider-based migration approach.
- Alternatively, maintain a single SQLite-compatible migration stream for dev and generate SQL Server migrations for CI/CD deployment.
- All migrations are code-first generated via `dotnet ef migrations add <Name>`.

### 5.2 Repository Pattern

```csharp
// Domain layer — interface
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
}

// Infrastructure layer — implementation
public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly TimeTrackerDbContext _context;
    // ...
}
```

Specialized repositories (e.g., `ITimeEntryRepository`) extend the base with query-specific methods.

### 5.3 Unit of Work

EF Core's `DbContext` inherently functions as a Unit of Work. Expose `SaveChangesAsync` via `IUnitOfWork` for explicit transaction control when needed.

---

## 6. API Layer — ASP.NET Controllers

### 6.1 Controller Structure

```
Controllers/
├── UnitsController.cs
├── CustomersController.cs
├── ProjectsController.cs
├── TasksController.cs
├── UsersController.cs
├── TimeEntriesController.cs
├── TimesheetsController.cs
└── ReportsController.cs
```

### 6.2 REST Endpoints

#### Units

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/units` | List units (paged, filtered) |
| GET | `/api/units/{id}` | Get unit by Id |
| POST | `/api/units` | Create unit |
| PUT | `/api/units/{id}` | Update unit |
| DELETE | `/api/units/{id}` | Archive unit |

#### Customers

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/customers` | List customers (paged, filtered) |
| GET | `/api/customers?unitId={id}` | List customers by unit |
| GET | `/api/customers/{id}` | Get customer by Id |
| POST | `/api/customers` | Create customer |
| PUT | `/api/customers/{id}` | Update customer |
| DELETE | `/api/customers/{id}` | Archive customer |

#### Projects

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/projects` | List projects (paged, filtered) |
| GET | `/api/projects?customerId={id}` | List root projects by customer |
| GET | `/api/projects/{id}` | Get project by Id |
| GET | `/api/projects/{id}/children` | Get child projects |
| GET | `/api/projects/{id}/hierarchy` | Get full project tree |
| GET | `/api/projects/{id}/users` | Get users assigned to project |
| GET | `/api/projects/tree` | Get complete hierarchy (all units/customers/projects) |
| POST | `/api/projects` | Create project (root or nested) |
| POST | `/api/projects/{id}/users` | Assign user(s) to project |
| PUT | `/api/projects/{id}` | Update project |
| DELETE | `/api/projects/{id}` | Archive project |
| DELETE | `/api/projects/{id}/users/{userId}` | Remove user from project |

#### Tasks

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/tasks` | List tasks (paged, filtered) |
| GET | `/api/tasks?projectId={id}` | List tasks by project |
| GET | `/api/tasks/{id}` | Get task by Id |
| GET | `/api/tasks/{id}/users` | Get users assigned to task |
| POST | `/api/tasks` | Create task |
| POST | `/api/tasks/{id}/users` | Assign user(s) to task |
| PUT | `/api/tasks/{id}` | Update task |
| DELETE | `/api/tasks/{id}` | Archive task |
| DELETE | `/api/tasks/{id}/users/{userId}` | Remove user from task |

#### Users

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/users` | List users (paged, filtered) |
| GET | `/api/users/{id}` | Get user by Id |
| GET | `/api/users/{id}/projects` | Get projects assigned to user |
| GET | `/api/users/{id}/tasks` | Get tasks assigned to user |
| POST | `/api/users` | Provision user (sync from Entra ID) |
| PUT | `/api/users/{id}` | Update user |
| DELETE | `/api/users/{id}` | Deactivate user |

#### Time Entries

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/time-entries` | List entries (paged, filtered by date/task/user) |
| GET | `/api/time-entries/{id}` | Get single entry |
| POST | `/api/time-entries` | Create entry (must reference a Task) |
| PUT | `/api/time-entries/{id}` | Update entry |
| DELETE | `/api/time-entries/{id}` | Delete entry |

#### Timesheets

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/timesheets/my` | Get current user's weekly timesheet |
| GET | `/api/timesheets/my?week=2026-02-09` | Get specific week |

#### Reports

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/reports/project/{id}` | Hours by project (aggregates all tasks) |
| GET | `/api/reports/task/{id}` | Hours by task |
| GET | `/api/reports/user/{id}` | Hours by user |
| GET | `/api/reports/overall` | Overall summary |

### 6.3 API Conventions

- All responses follow **ProblemDetails** (RFC 9457) for errors.
- Pagination via `?page=1&pageSize=20` query parameters, returns `X-Pagination` header.
- API versioning via URL path prefix (future-proofing): `/api/v1/...`. Use version 1 if version not specified.
- Swagger/OpenAPI via Swashbuckle.

---

## 7. Frontend — React 19

### 7.1 Tech Stack

| Library | Purpose |
|---------|---------|
| React 19 | UI framework |
| React Router 7 | Client-side routing |
| Material UI 6 | Component library |
| TanStack Query 5 | Server-state management |
| Axios | HTTP client |
| MSAL React | Microsoft Authentication Library for Entra ID auth |
| React Hook Form + Zod | Form handling + validation |
| Day.js | Date manipulation |
| Recharts | Charts for reports |
| Vite 6 | Build & dev server |

### 7.2 MUI Theme

```typescript
import { createTheme } from '@mui/material/styles';

const theme = createTheme({
  palette: {
    primary: {
      main: '#17428c',
    },
    secondary: {
      main: '#009dc3',
    },
    background: {
      default: '#ffffff',
      paper: '#f8f9fc',
    },
  },
  typography: {
    fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
  },
  shape: {
    borderRadius: 8,
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: { textTransform: 'none', fontWeight: 600 },
      },
    },
    MuiAppBar: {
      defaultProps: { elevation: 0 },
      styleOverrides: {
        root: { borderBottom: '1px solid #e0e0e0' },
      },
    },
  },
});
```

### 7.3 Application Pages

```
/                          → Dashboard (redirect if logged in)
/login                     → Login page
/dashboard                 → Overview: weekly hours, recent entries, quick actions
/timesheet                 → Weekly timesheet grid (current week)
/timesheet?week=2026-02-09 → Specific week
/time-entries              → Full list of time entries (filterable)
/time-entries/new          → Create time entry
/time-entries/:id/edit     → Edit time entry
/units                     → Unit list (Admin/Manager)
/units/new                 → Create unit (Admin)
/units/:id                 → Unit detail with customers
/customers                 → Customer list (Admin/Manager)
/customers/new             → Create customer (Admin/Manager)
/customers/:id             → Customer detail with projects
/projects                  → Project hierarchy tree view
/projects/new              → Create project (root or nested)
/projects/:id              → Project detail, children, tasks, & hours summary
/projects/:id/users        → Manage users assigned to project
/projects/:id/projects     → List child projects
/projects/:id/projects/new → Create child project
/projects/:id/tasks        → List tasks for project
/projects/:id/tasks/new    → Create task
/projects/:id/tasks/:id    → Task detail & hours summary
/projects/:id/tasks/:id/edit            → Edit task
/projects/:id/tasks/:id/users           → Manage users assigned to task
/users/:id/projects        → View user's project assignments
/users/:id/tasks           → View user's task assignments
/reports                   → Reports dashboard
/reports/project/:id       → Project report (charts + table, includes children)
/reports/user/:id          → User report
/reports/overall           → Overall summary report
/admin/users               → User management (Admin)
/profile                   → Current user profile
```

### 7.4 Key UI Components

```
components/
├── layout/
│   ├── AppShell.tsx            # Sidebar + AppBar + content area
│   ├── Sidebar.tsx             # Navigation drawer
│   └── TopBar.tsx              # App bar with user menu
├── timesheet/
│   ├── TimesheetGrid.tsx       # Weekly grid (Mon-Sun × projects)
│   ├── TimesheetRow.tsx        # Single project row
│   └── DayCell.tsx             # Editable hour cell
├── time-entries/
│   ├── TimeEntryForm.tsx       # Create/edit form with task picker
│   ├── TimeEntryList.tsx       # Filterable list
│   └── TimeEntryCard.tsx       # Entry summary card
├── hierarchy/
│   ├── HierarchyTree.tsx       # Full Unit→Customer→Project tree (MUI TreeView)
│   ├── HierarchyBreadcrumb.tsx # Breadcrumb navigation
│   ├── ProjectTreeNode.tsx     # Recursive project node component
│   └── ProjectPicker.tsx       # Hierarchical dropdown for forms
├── units/
│   ├── UnitForm.tsx
│   ├── UnitList.tsx
│   └── UnitCard.tsx
├── customers/
│   ├── CustomerForm.tsx
│   ├── CustomerList.tsx
│   └── CustomerCard.tsx
├── projects/
│   ├── ProjectForm.tsx
│   ├── ProjectList.tsx
│   ├── ProjectCard.tsx
│   ├── ProjectHierarchyView.tsx # Tree view for nested projects
│   ├── ProjectUserList.tsx      # Users assigned to project
│   ├── ProjectUserAssignment.tsx # Assign/remove users dialog
│   └── ProjectTaskList.tsx      # Tasks for project
├── tasks/
│   ├── TaskForm.tsx
│   ├── TaskList.tsx
│   ├── TaskCard.tsx
│   ├── TaskPicker.tsx           # Task selector for time entry forms
│   ├── TaskUserList.tsx         # Users assigned to task
│   └── TaskUserAssignment.tsx   # Assign/remove users dialog
├── users/
│   ├── UserForm.tsx
│   ├── UserList.tsx
│   ├── UserCard.tsx
│   ├── UserProjectList.tsx      # Projects assigned to user
│   └── UserTaskList.tsx         # Tasks assigned to user
├── reports/
│   ├── HoursBarChart.tsx       # Recharts bar chart
│   ├── HoursPieChart.tsx       # Recharts pie chart
│   ├── ReportFilters.tsx       # Date range, project, user filters
│   └── ReportTable.tsx         # Tabular data export
├── common/
│   ├── PageHeader.tsx
│   ├── ConfirmDialog.tsx
│   ├── EmptyState.tsx
│   └── LoadingOverlay.tsx
└── auth/
    ├── LoginForm.tsx
    └── ProtectedRoute.tsx
```

### 7.5 Frontend Folder Structure

```
src/app/
├── index.html
├── vite.config.ts
├── tsconfig.json
├── package.json
├── public/
│   └── favicon.svg
└── src/
    ├── main.tsx
    ├── App.tsx
    ├── theme.ts
    ├── api/
    │   ├── client.ts            # Axios instance + interceptors
    │   ├── auth.api.ts
    │   ├── units.api.ts
    │   ├── customers.api.ts
    │   ├── projects.api.ts
    │   ├── tasks.api.ts
    │   ├── timeEntries.api.ts
    │   ├── timesheets.api.ts
    │   ├── reports.api.ts
    │   └── users.api.ts
    ├── hooks/
    │   ├── useAuth.ts
    │   ├── useUnits.ts
    │   ├── useCustomers.ts
    │   ├── useProjects.ts
    │   ├── useProjectHierarchy.ts
    │   ├── useProjectUsers.ts
    │   ├── useTasks.ts
    │   ├── useTaskUsers.ts
    │   ├── useUserProjects.ts
    │   ├── useUsers.ts
    │   ├── useUserTasks.ts
    │   ├── useTimeEntries.ts
    │   └── useReports.ts
    ├── components/              # (see §7.4)
    ├── pages/                   # Route-level components
    ├── types/                   # TypeScript interfaces
    │   ├── auth.types.ts
    │   ├── unit.types.ts
    │   ├── customer.types.ts
    │   ├── project.types.ts
    │   ├── task.types.ts
    │   ├── projectUser.types.ts
    │   ├── taskUser.types.ts
    │   ├── user.types.ts
    │   ├── hierarchy.types.ts
    │   ├── timeEntry.types.ts
    │   ├── report.types.ts
    │   └── common.types.ts
    ├── utils/
    │   ├── dates.ts
    │   └── formatting.ts
    └── contexts/
        └── AuthContext.tsx
```

---

## 8. Authentication & Authorization

| Concern | Approach |
|---------|----------|
| Identity provider | Microsoft Entra ID (Azure Active Directory) |
| Token format | OAuth 2.0 / OpenID Connect tokens issued by Entra ID |
| User store | Entra ID tenant |
| Role-based access | `[Authorize(Roles = "Admin")]` on controllers/actions; roles stored in User entity |
| Frontend authentication | MSAL.js (Microsoft Authentication Library) for browser-based auth flow |
| Backend validation | Microsoft.Identity.Web validates Entra ID tokens |

**Authentication Flow:**
1. User authenticates via Entra ID through the frontend (MSAL.js)
2. Entra ID issues OAuth 2.0 / OpenID Connect tokens
3. Frontend includes token in API requests (Authorization header)
4. Backend validates token signature and claims using Microsoft.Identity.Web
5. User's Entra ID identity (email/objectId) is mapped to local User entity
6. Local User entity stores role (Admin/Manager/Employee) for authorization

**Note:** User records may be pre-provisioned by admins or auto-created on first login. Entra ID handles all password management and MFA.

### Role Permissions Matrix

| Action | Employee | Manager | Admin |
|--------|----------|---------|-------|
| Log own time | ✅ | ✅ | ✅ |
| Edit/delete own entries | ✅ | ✅ | ✅ |
| View own entries | ✅ | ✅ | ✅ |
| View all entries | ❌ | ✅ | ✅ |
| Edit/delete any entry | ❌ | ✅ | ✅ |
| View hierarchy | ❌ | ✅ | ✅ |
| View project assignments | ❌ | ✅ | ✅ |
| Assign users to projects | ❌ | ✅ | ✅ |
| Manage projects | ❌ | ✅ | ✅ |
| Manage customers | ❌ | ✅ | ✅ |
| Manage units | ❌ | ❌ | ✅ |
| View reports | ❌ | ✅ | ✅ |
| Manage users | ❌ | ❌ | ✅ |

---

## 9. NuGet Packages

### TimeTracker.Domain

```
(no external packages — pure C#)
```

### TimeTracker.Application

```xml
<PackageReference Include="MediatR" Version="12.5.0" />
<PackageReference Include="FluentValidation" Version="11.*" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.*" />
<PackageReference Include="AutoMapper" Version="13.*" />
```

### TimeTracker.Infrastructure

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.*" />
```

### TimeTracker

**Note:** This is the main API project (folder name is `TimeTracker/` in the solution structure).

```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.*" />
<PackageReference Include="Microsoft.Identity.Web" Version="3.*" />
```

---

## 10. Implementation Phases

### Phase 1 — Foundation (Week 1–2)

| # | Task | Deliverable |
|---|------|-------------|
| 1.1 | Create solution & project scaffolding | `.sln` + 4 projects |
| 1.2 | Define domain entities (Unit, Customer, Project w/ hierarchy, Task, User, TimeEntry) | Domain project compiles |
| 1.3 | Configure EF Core DbContext + self-referencing Project relationship | Infrastructure project |
| 1.4 | Generate initial migration (SQLite) | Dev DB created |
| 1.5 | Implement generic repository + unit of work | Infrastructure project |
| 1.6 | Wire up DI (MediatR, FluentValidation, EF, repos) | API `Program.cs` |
| 1.7 | Scaffold React app with Vite + MUI theme | Web project builds |
| 1.8 | Build AppShell layout (sidebar, topbar, routing) | Navigation works |

### Phase 2 — Core Time Tracking (Week 3–4)

| # | Task | Deliverable |
|---|------|-------------|
| 2.1 | Configure Entra ID authentication (backend + frontend) | API validates Entra ID tokens |
| 2.2 | Frontend MSAL integration + auth context | Protected routes work |
| 2.3 | CRUD commands/queries for Units & Customers | `UnitsController`, `CustomersController` |
| 2.4 | CRUD commands/queries for Projects (with hierarchy support) | `ProjectsController` |
| 2.5 | CRUD commands/queries for Tasks | `TasksController` |
| 2.6 | Frontend: Unit & Customer management (Admin/Manager) | Unit & Customer pages |
| 2.7 | Frontend: Project hierarchy tree view & task management | Project tree & task components |
| 2.8 | User-Project and User-Task assignment (commands/queries) | Assignment endpoints |
| 2.9 | CRUD commands/queries for TimeEntries | `TimeEntriesController` |
| 2.10 | Frontend: Time entry form with task picker | Time entry pages |
| 2.11 | Frontend: Project and task user management UI | Project & task assignments |
| 2.12 | Weekly timesheet grid (API + UI) | Timesheet page |

### Phase 3 — Reporting & Polish (Week 5–6)

| # | Task | Deliverable |
|---|------|-------------|
| 3.1 | Report queries (project, user, org) | `ReportsController` |
| 3.2 | Frontend report pages with charts | Report pages |
| 3.3 | Dashboard with summary widgets | Dashboard page |
| 3.4 | User management (Admin) | User CRUD |
| 3.5 | Validation pipeline + error handling | ProblemDetails globally |
| 3.6 | Pagination, filtering, sorting | Consistent UX |
| 3.7 | Swagger documentation polish | OpenAPI spec complete |

### Phase 4 — Hardening (Week 7–8)

| # | Task | Deliverable |
|---|------|-------------|
| 4.1 | Integration tests: API endpoints | Happy-path coverage |
| 4.2 | SQL Server migration + deployment config | Production-ready |
| 4.3 | Performance: query optimization, indexes | Responsive under load |
| 4.4 | Security review: CORS, rate limiting, input validation | Hardened |

---

## 11. Development Workflow

### Running the API (Development)

```bash
cd src/api/TimeTracker
dotnet run
# API available at https://localhost:5001
# Swagger at https://localhost:5001/swagger
```

### Running the Frontend

```bash
cd src/app
npm install
npm run dev
# App available at http://localhost:5173
# Proxies /api/* to https://localhost:5001
```

### Database Migrations

```bash
cd src/api
# Add migration
dotnet ef migrations add <MigrationName> \
  --project TimeTracker.Infrastructure \
  --startup-project TimeTracker

# Apply migration
dotnet ef database update \
  --project TimeTracker.Infrastructure \
  --startup-project TimeTracker
```

### Environment Configuration

```jsonc
// appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=timetracker.db"  // SQLite
  },
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "Audience": "api://your-api-client-id"
  }
}
```

```jsonc
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=TimeTracker;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## 12. Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| **MediatR 12.5.0** | Last MIT-licensed version; v13+ requires commercial license |
| **SQLite for dev** | Zero-install local development; EF Core abstraction makes provider swap seamless |
| **Generic repository** | Reduces boilerplate; specialized repos extend where needed |
| **MUI 6** | Enterprise-grade component library with excellent theming support |
| **TanStack Query** | Handles server-state caching, background refresh, and optimistic updates |
| **Vite** | Fastest React dev server & build tool |
| **Entra ID auth** | Enterprise SSO, centralized user management, no password storage in app |
| **FluentValidation** | Expressive, testable validation rules decoupled from handlers |
| **DateOnly for time entries** | Entries are day-scoped, not timestamp-scoped |
| **No approval workflow** | Simplified model: users create/edit/delete their entries; managers can view/manage all |
| **Hierarchical projects** | Unit→Customer→Project→Project structure enables organizational flexibility and detailed reporting |
| **Self-referencing Projects** | Parent-child relationships via `ParentId` allow unlimited nesting depth |
| **Task-based time tracking** | Only Tasks (leaf nodes) can receive time entries; Projects organize Tasks hierarchically |
| **User-Project assignments** | Many-to-many relationship tracks project team composition; assignments are optional for time tracking |

---

## 13. Future Enhancements (Out of Scope)

- Export reports to PDF/Excel
- Integration with calendar apps (Outlook, Google Calendar)
- Timer/stopwatch for real-time tracking
- Multi-organization support
- Notification system (email, in-app)
- Audit log for all changes
- Mobile-responsive PWA
- Dark mode theme variant
