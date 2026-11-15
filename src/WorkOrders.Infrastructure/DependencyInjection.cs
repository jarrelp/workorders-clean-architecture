using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkOrders.Application.Abstractions;
using WorkOrders.Infrastructure.Persistence;
using WorkOrders.Infrastructure.Time;

namespace WorkOrders.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "WorkOrders";

    /// <summary>Registers the SQLite database, the implementations of the application ports and a database health check.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // The connection string is read when the first DbContext is created, after all
        // configuration sources (including test overrides) have been added.
        // A work order owns two collections. Split queries load each in its own query, instead of
        // one join that returns every time entry combined with every material line.
        services.AddDbContext<WorkOrdersDbContext>((provider, options) =>
            options.UseSqlite(
                provider.GetRequiredService<IConfiguration>().GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured."),
                sqlite => sqlite.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
        services.AddScoped<IWorkOrderNumberGenerator, WorkOrderNumberGenerator>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IClock, SystemClock>();

        services.AddHealthChecks().AddDbContextCheck<WorkOrdersDbContext>();

        return services;
    }

    /// <summary>
    /// Creates the database and its tables if they do not exist yet. Fine for a sample;
    /// a production system would apply EF Core migrations from its deployment pipeline instead.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WorkOrdersDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }
}
