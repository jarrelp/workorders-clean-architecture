using WorkOrders.Application.Abstractions;
using WorkOrders.Application.Validation;
using WorkOrders.Domain;
using WorkOrders.Domain.Common;

namespace WorkOrders.Application.UseCases;

public sealed record LogTimeCommand(Guid WorkOrderId, string Technician, DateOnly Date, int Minutes);

/// <summary>Records time a technician spent on a work order that is in progress.</summary>
public sealed class LogTimeHandler(IWorkOrderRepository workOrders, IUnitOfWork unitOfWork)
{
    public async Task<Result<WorkOrderResponse>> HandleAsync(LogTimeCommand command, CancellationToken cancellationToken)
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

        var logged = workOrder.LogTime(command.Technician, command.Date, command.Minutes);
        if (logged.IsFailure)
        {
            return logged.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return workOrder.ToResponse();
    }

    private static Result Validate(LogTimeCommand command) =>
        new RequestValidator()
            .RequireText(nameof(command.Technician), command.Technician, FieldLimits.Technician)
            .RequireGreaterThanZero(nameof(command.Minutes), command.Minutes)
            .ToResult();
}
