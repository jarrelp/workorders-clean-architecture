using WorkOrders.Application.Abstractions;

namespace WorkOrders.Infrastructure.Persistence;

/// <summary>One SaveChanges per use case: EF Core writes all tracked changes in one transaction.</summary>
internal sealed class UnitOfWork(WorkOrdersDbContext dbContext) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
