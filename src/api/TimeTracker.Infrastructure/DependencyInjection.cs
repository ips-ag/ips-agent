using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Domain.Interfaces;
using TimeTracker.Infrastructure.Data;
using TimeTracker.Infrastructure.Repositories;

namespace TimeTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool isDevelopment = true)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=timetracker.db";

        if (isDevelopment)
        {
            services.AddDbContext<TimeTrackerDbContext>(options =>
                options.UseSqlite(connectionString));
        }
        else
        {
            services.AddDbContext<TimeTrackerDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TimeTrackerDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        return services;
    }
}
