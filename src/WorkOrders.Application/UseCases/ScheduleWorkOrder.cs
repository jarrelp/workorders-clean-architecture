using WorkOrders.Application.Abstractions;
using WorkOrders.Application.Validation;
using WorkOrders.Domain;
using WorkOrders.Domain.Common;

namespace WorkOrders.Application.UseCases;

public sealed record ScheduleWorkOrderCommand(Guid WorkOrderId, string Technician, DateOnly ScheduledDate);

/// <summary>Assigns a technician and a visit date. The clock decides what "not in the past" means.</summary>
public sealed class ScheduleWorkOrderHandler(IWorkOrderRepository workOrders, IClock clock, IUnitOfWork unitOfWork)
{
    public async Task<Result<WorkOrderResponse>> HandleAsync(ScheduleWorkOrderCommand command, CancellationToken cancellationToken)
    {
        var validation = Validate(command);
        if (validation.IsFailure)
        {
            return validation.Error;
        }

        var workOrder = await workOrders.GetByIdAsync(command.WorkOrderId, cancellationToken);
        if (workOrder is null)
        {
            return WorkOrderErrors.NotFound(command.WorkOrderId);
        }

        var scheduled = workOrder.Schedule(command.Technician, command.ScheduledDate, clock.Today);
        if (scheduled.IsFailure)
        {
            return scheduled.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return workOrder.ToResponse();
    }

    private static Result Validate(ScheduleWorkOrderCommand command) =>
        new RequestValidator()
            .RequireText(nameof(command.Technician), command.Technician, FieldLimits.Technician)
            .ToResult();
}
