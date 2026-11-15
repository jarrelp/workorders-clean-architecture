namespace WorkOrders.Application.UseCases;

/// <summary>One row in a list of work orders.</summary>
public sealed record WorkOrderSummaryResponse(
    Guid Id,
    string Number,
    string CustomerName,
    string Status,
    DateOnly? ScheduledDate,
    string? AssignedTechnician,
    decimal Total);
