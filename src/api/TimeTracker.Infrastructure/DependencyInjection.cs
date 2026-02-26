using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Domain.Interfaces;
using TimeTracker.Infrastructure.Data;
using TimeTracker.Infrastructure.Repositories;

namespace TimeTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<TimeTrackerDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TimeTrackerDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        return services;
    }
}
