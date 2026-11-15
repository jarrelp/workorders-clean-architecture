using WorkOrders.Domain;

namespace WorkOrders.Application.Abstractions;

/// <summary>Loads and stores whole <see cref="WorkOrder"/> aggregates, including their time entries and materials.</summary>
public interface IWorkOrderRepository
{
    Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Returns the work orders ordered by number, optionally only those with the given status.</summary>
    Task<IReadOnlyList<WorkOrder>> ListAsync(WorkOrderStatus? status, CancellationToken cancellationToken);

    /// <summary>Adds a new work order. It is stored when the unit of work saves.</summary>
    void Add(WorkOrder workOrder);
}
