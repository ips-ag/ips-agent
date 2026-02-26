using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TimeTracker.Infrastructure.Data;

public class TimeTrackerDbContextFactory : IDesignTimeDbContextFactory<TimeTrackerDbContext>
{
    public TimeTrackerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TimeTrackerDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=TimeTracker;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True",
            options => options.EnableRetryOnFailure());
        return new TimeTrackerDbContext(optionsBuilder.Options);
    }
}
