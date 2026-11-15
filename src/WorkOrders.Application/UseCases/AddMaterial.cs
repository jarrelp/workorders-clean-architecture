using WorkOrders.Application.Abstractions;
using WorkOrders.Application.Validation;
using WorkOrders.Domain;
using WorkOrders.Domain.Common;

namespace WorkOrders.Application.UseCases;

public sealed record AddMaterialCommand(Guid WorkOrderId, string Description, decimal Quantity, decimal UnitPrice);

/// <summary>Adds used material to a work order that is in progress.</summary>
public sealed class AddMaterialHandler(IWorkOrderRepository workOrders, IUnitOfWork unitOfWork)
{
    public async Task<Result<WorkOrderResponse>> HandleAsync(AddMaterialCommand command, CancellationToken cancellationToken)
    {
        var validation = Validate(command);
        if (validation.IsFailure)
        {
            return validation.Error;
        }

        var unitPrice = Money.FromEuros(command.UnitPrice);
        if (unitPrice.IsFailure)
        {
            return unitPrice.Error;
        }

        var workOrder = await workOrders.GetByIdAsync(command.WorkOrderId, cancellationToken);
        if (workOrder is null)
        {
            return WorkOrderErrors.NotFound(command.WorkOrderId);
        }

        var added = workOrder.AddMaterial(command.Description, command.Quantity, unitPrice.Value);
        if (added.IsFailure)
        {
            return added.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return workOrder.ToResponse();
    }

    private static Result Validate(AddMaterialCommand command) =>
        new RequestValidator()
            .RequireText(nameof(command.Description), command.Description, FieldLimits.MaterialDescription)
            .RequireGreaterThanZero(nameof(command.Quantity), command.Quantity)
            .RequireNotNegative(nameof(command.UnitPrice), command.UnitPrice)
            .ToResult();
}
