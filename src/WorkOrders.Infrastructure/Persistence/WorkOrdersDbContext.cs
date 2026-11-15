using Microsoft.EntityFrameworkCore;
using WorkOrders.Domain;
using WorkOrders.Domain.Common;
using WorkOrders.Infrastructure.Persistence.Configurations;

namespace WorkOrders.Infrastructure.Persistence;

internal sealed class WorkOrdersDbContext(DbContextOptions<WorkOrdersDbContext> options) : DbContext(options)
{
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    public DbSet<WorkOrderNumberSequence> WorkOrderNumberSequences => Set<WorkOrderNumberSequence>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.Properties<Money>().HaveConversion<MoneyConverter>().HavePrecision(18, 2);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new WorkOrderConfiguration());
        modelBuilder.ApplyConfiguration(new WorkOrderNumberSequenceConfiguration());
    }
}
