using WorkOrders.Application.Abstractions;
using WorkOrders.Domain;

namespace WorkOrders.Application.Tests.Fakes;

/// <summary>A repository backed by a dictionary. Enough to test the handlers without a database.</summary>
internal sealed class InMemoryWorkOrderRepository : IWorkOrderRepository
{
    private readonly Dictionary<Guid, WorkOrder> _workOrders = [];

    public IReadOnlyCollection<WorkOrder> All => _workOrders.Values;

    public Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_workOrders.GetValueOrDefault(id));

    public Task<IReadOnlyList<WorkOrder>> ListAsync(WorkOrderStatus? status, CancellationToken cancellationToken)
    {
        IReadOnlyList<WorkOrder> found =
        [
            .. _workOrders.Values
                .Where(workOrder => status is null || workOrder.Status == status)
                .OrderBy(workOrder => workOrder.Number.Value, StringComparer.Ordinal),
        ];

        return Task.FromResult(found);
    }

    public void Add(WorkOrder workOrder) => _workOrders.Add(workOrder.Id, workOrder);
}
