using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WorkOrders.Application.Abstractions;
using WorkOrders.Infrastructure;

namespace WorkOrders.Api.Tests;

/// <summary>
/// Runs the real API in memory, with the real EF Core mapping on an in-memory SQLite database.
/// Only the clock is replaced, so tests about dates do not depend on the day they run.
/// </summary>
public sealed class WorkOrdersApiFactory : WebApplicationFactory<Program>
{
    public static readonly DateOnly Today = new(2026, 3, 2);

    // A named in-memory database exists as long as one connection to it is open.
    // The unique name gives every factory, and so every test class, its own database.
    private readonly string _connectionString = $"Data Source=workorders-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
    private readonly SqliteConnection _keepAlive;

    public WorkOrdersApiFactory()
    {
        _keepAlive = new SqliteConnection(_connectionString);
        _keepAlive.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting($"ConnectionStrings:{DependencyInjection.ConnectionStringName}", _connectionString);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IClock>();
            services.AddSingleton<IClock>(new FixedClock(Today));
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _keepAlive.Dispose();
        }

        base.Dispose(disposing);
    }

    private sealed class FixedClock(DateOnly today) : IClock
    {
        public DateOnly Today { get; } = today;
    }
}
