using WorkOrders.Application.Abstractions;
using WorkOrders.Domain;
using WorkOrders.Domain.Common;

namespace WorkOrders.Application.UseCases;

public sealed record CancelWorkOrderCommand(Guid WorkOrderId);

/// <summary>Calls off a work order that is not completed yet. After this it cannot change.</summary>
public sealed class CancelWorkOrderHandler(IWorkOrderRepository workOrders, IUnitOfWork unitOfWork)
{
    public async Task<Result<WorkOrderResponse>> HandleAsync(CancelWorkOrderCommand command, CancellationToken cancellationToken)
    {
        var workOrder = await workOrders.GetByIdAsync(command.WorkOrderId, cancellationToken);
        if (workOrder is null)
        {
            return WorkOrderErrors.NotFound(command.WorkOrderId);
        }

        var cancelled = workOrder.Cancel();
        if (cancelled.IsFailure)
        {
            return cancelled.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return workOrder.ToResponse();
    }
}
