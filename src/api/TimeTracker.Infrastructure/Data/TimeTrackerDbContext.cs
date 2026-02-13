using Microsoft.EntityFrameworkCore;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Infrastructure.Data;

public class TimeTrackerDbContext : DbContext, IUnitOfWork
{
    public TimeTrackerDbContext(DbContextOptions<TimeTrackerDbContext> options) : base(options) { }

    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTask> Tasks => Set<ProjectTask>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ProjectUser> ProjectUsers => Set<ProjectUser>();
    public DbSet<TaskUser> TaskUsers => Set<TaskUser>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TimeTrackerDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var now = DateTimeOffset.UtcNow;
            if (entry.State == EntityState.Added)
            {
                if (entry.Metadata.FindProperty("CreatedAt") != null)
                    entry.Property("CreatedAt").CurrentValue = now;
            }
            if (entry.Metadata.FindProperty("UpdatedAt") != null)
                entry.Property("UpdatedAt").CurrentValue = now;
        }

        return await base.SaveChangesAsync(ct);
    }
}
