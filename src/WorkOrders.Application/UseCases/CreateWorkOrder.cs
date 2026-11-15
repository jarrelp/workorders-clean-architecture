using WorkOrders.Application.Abstractions;
using WorkOrders.Application.Validation;
using WorkOrders.Domain;
using WorkOrders.Domain.Common;

namespace WorkOrders.Application.UseCases;

public sealed record CreateWorkOrderCommand(
    string CustomerName,
    string SiteDescription,
    string ProblemDescription,
    decimal HourlyRate);

/// <summary>Registers a reported problem as a new work order in Draft, numbered within the current year.</summary>
public sealed class CreateWorkOrderHandler(
    IWorkOrderRepository workOrders,
    IWorkOrderNumberGenerator numberGenerator,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<WorkOrderResponse>> HandleAsync(CreateWorkOrderCommand command, CancellationToken cancellationToken)
    {
        var validation = Validate(command);
        if (validation.IsFailure)
        {
            return validation.Error;
        }

        var hourlyRate = Money.FromEuros(command.HourlyRate);
        if (hourlyRate.IsFailure)
        {
            return hourlyRate.Error;
        }

        var number = await numberGenerator.NextAsync(clock.Today.Year, cancellationToken);
        var workOrder = WorkOrder.Create(
            number,
            command.CustomerName,
            command.SiteDescription,
            command.ProblemDescription,
            hourlyRate.Value);
        if (workOrder.IsFailure)
        {
            return workOrder.Error;
        }

        workOrders.Add(workOrder.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return workOrder.Value.ToResponse();
    }

    private static Result Validate(CreateWorkOrderCommand command) =>
        new RequestValidator()
            .RequireText(nameof(command.CustomerName), command.CustomerName, FieldLimits.CustomerName)
            .RequireText(nameof(command.SiteDescription), command.SiteDescription, FieldLimits.SiteDescription)
            .RequireText(nameof(command.ProblemDescription), command.ProblemDescription, FieldLimits.ProblemDescription)
            .RequireNotNegative(nameof(command.HourlyRate), command.HourlyRate)
            .ToResult();
}
