using WorkOrders.Domain;
using WorkOrders.Domain.Common;

namespace WorkOrders.Application.UseCases;

/// <summary>Maps the domain model to response contracts. Written out by hand, so every field is visible.</summary>
internal static class WorkOrderMapping
{
    public static WorkOrderResponse ToResponse(this WorkOrder workOrder) => new(
        workOrder.Id,
        workOrder.Number.Value,
        workOrder.CustomerName,
        workOrder.SiteDescription,
        workOrder.ProblemDescription,
        workOrder.Status.ToString(),
        workOrder.HourlyRate.Amount,
        workOrder.ScheduledDate,
        workOrder.AssignedTechnician,
        [.. workOrder.TimeEntries.Select(entry => new TimeEntryResponse(entry.Technician, entry.Date, entry.Minutes))],
        [.. workOrder.MaterialLines.Select(line => new MaterialLineResponse(line.Description, line.Quantity, line.UnitPrice.Amount, line.LineTotal.Amount))],
        new WorkOrderTotalsResponse(workOrder.LabourTotal.Amount, workOrder.MaterialsTotal.Amount, workOrder.Total.Amount, Money.Currency));

    public static WorkOrderSummaryResponse ToSummary(this WorkOrder workOrder) => new(
        workOrder.Id,
        workOrder.Number.Value,
        workOrder.CustomerName,
        workOrder.Status.ToString(),
        workOrder.ScheduledDate,
        workOrder.AssignedTechnician,
        workOrder.Total.Amount);
}
