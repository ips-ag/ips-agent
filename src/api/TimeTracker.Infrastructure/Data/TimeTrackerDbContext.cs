using Microsoft.EntityFrameworkCore;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Infrastructure.Data;

public class TimeTrackerDbContext : DbContext, IUnitOfWork
{
    public DbSet<Unit> Units
    {
        get => Set<Unit>();
    }

    public DbSet<Customer> Customers
    {
        get => Set<Customer>();
    }

    public DbSet<Project> Projects
    {
        get => Set<Project>();
    }

    public DbSet<ProjectTask> Tasks
    {
        get => Set<ProjectTask>();
    }

    public DbSet<User> Users
    {
        get => Set<User>();
    }

    public DbSet<ProjectUser> ProjectUsers
    {
        get => Set<ProjectUser>();
    }

    public DbSet<TaskUser> TaskUsers
    {
        get => Set<TaskUser>();
    }

    public DbSet<TimeEntry> TimeEntries
    {
        get => Set<TimeEntry>();
    }

    public TimeTrackerDbContext(DbContextOptions<TimeTrackerDbContext> options) : base(options) { }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var entries = ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified);
        foreach (var entry in entries)
        {
            var now = DateTimeOffset.UtcNow;
            if (entry.State == EntityState.Added)
            {
                if (entry.Metadata.FindProperty("CreatedAt") != null) entry.Property("CreatedAt").CurrentValue = now;
            }
            if (entry.Metadata.FindProperty("UpdatedAt") != null) entry.Property("UpdatedAt").CurrentValue = now;
        }
        return await base.SaveChangesAsync(ct);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TimeTrackerDbContext).Assembly);
    }
}
