using Microsoft.EntityFrameworkCore;
using WorkOrders.Application.Abstractions;
using WorkOrders.Domain;

namespace WorkOrders.Infrastructure.Persistence;

/// <summary>Time entries and material lines are owned by the work order, so EF Core always loads them with it.</summary>
internal sealed class WorkOrderRepository(WorkOrdersDbContext dbContext) : IWorkOrderRepository
{
    public Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.WorkOrders.SingleOrDefaultAsync(workOrder => workOrder.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WorkOrder>> ListAsync(WorkOrderStatus? status, CancellationToken cancellationToken)
    {
        var query = dbContext.WorkOrders.AsNoTracking();
        if (status is { } wanted)
        {
            query = query.Where(workOrder => workOrder.Status == wanted);
        }

        return await query.OrderBy(workOrder => workOrder.Number).ToListAsync(cancellationToken);
    }

    public void Add(WorkOrder workOrder) => dbContext.WorkOrders.Add(workOrder);
}
