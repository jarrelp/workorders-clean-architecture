using WorkOrders.Application.Abstractions;
using WorkOrders.Domain;
using WorkOrders.Domain.Common;

namespace WorkOrders.Application.UseCases;

public sealed record GetWorkOrderQuery(Guid WorkOrderId);

/// <summary>Returns one work order with its time entries, materials and totals.</summary>
public sealed class GetWorkOrderHandler(IWorkOrderRepository workOrders)
{
    public async Task<Result<WorkOrderResponse>> HandleAsync(GetWorkOrderQuery query, CancellationToken cancellationToken)
    {
        var workOrder = await workOrders.GetByIdAsync(query.WorkOrderId, cancellationToken);

        return workOrder is null
            ? WorkOrderErrors.NotFound(query.WorkOrderId)
            : workOrder.ToResponse();
    }
}
