using WorkOrders.Application.Abstractions;
using WorkOrders.Domain;
using WorkOrders.Domain.Common;

namespace WorkOrders.Application.UseCases;

public sealed record CompleteWorkOrderCommand(Guid WorkOrderId);

/// <summary>Finishes a work order that is in progress and has time logged. After this it cannot change.</summary>
public sealed class CompleteWorkOrderHandler(IWorkOrderRepository workOrders, IUnitOfWork unitOfWork)
{
    public async Task<Result<WorkOrderResponse>> HandleAsync(CompleteWorkOrderCommand command, CancellationToken cancellationToken)
    {
        var workOrder = await workOrders.GetByIdAsync(command.WorkOrderId, cancellationToken);
        if (workOrder is null)
        {
            return WorkOrderErrors.NotFound(command.WorkOrderId);
        }

        var completed = workOrder.Complete();
        if (completed.IsFailure)
        {
            return completed.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return workOrder.ToResponse();
    }
}
