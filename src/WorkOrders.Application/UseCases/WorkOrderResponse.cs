namespace WorkOrders.Application.UseCases;

/// <summary>A work order as returned to clients. Amounts are in euros with two decimals.</summary>
public sealed record WorkOrderResponse(
    Guid Id,
    string Number,
    string CustomerName,
    string SiteDescription,
    string ProblemDescription,
    string Status,
    decimal HourlyRate,
    DateOnly? ScheduledDate,
    string? AssignedTechnician,
    IReadOnlyList<TimeEntryResponse> TimeEntries,
    IReadOnlyList<MaterialLineResponse> MaterialLines,
    WorkOrderTotalsResponse Totals);

public sealed record TimeEntryResponse(string Technician, DateOnly Date, int Minutes);

public sealed record MaterialLineResponse(string Description, decimal Quantity, decimal UnitPrice, decimal LineTotal);

public sealed record WorkOrderTotalsResponse(decimal Labour, decimal Materials, decimal Total, string Currency);
