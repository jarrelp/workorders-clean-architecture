using WorkOrders.Application.Abstractions;
using WorkOrders.Domain;
using WorkOrders.Domain.Common;

namespace WorkOrders.Application.UseCases;

public sealed record StartWorkOrderCommand(Guid WorkOrderId);

/// <summary>Marks a scheduled work order as in progress: the technician has started on site.</summary>
public sealed class StartWorkOrderHandler(IWorkOrderRepository workOrders, IUnitOfWork unitOfWork)
{
    public async Task<Result<WorkOrderResponse>> HandleAsync(StartWorkOrderCommand command, CancellationToken cancellationToken)
    {
        var workOrder = await workOrders.GetByIdAsync(command.WorkOrderId, cancellationToken);
        if (workOrder is null)
        {
            return WorkOrderErrors.NotFound(command.WorkOrderId);
        }

        var started = workOrder.Start();
        if (started.IsFailure)
        {
            return started.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return workOrder.ToResponse();
    }
}
