namespace WorkOrders.Api.Endpoints;

// Request bodies. The work order id comes from the route, so it is not part of the body.

internal sealed record CreateWorkOrderRequest(
    string CustomerName,
    string SiteDescription,
    string ProblemDescription,
    decimal HourlyRate);

internal sealed record ScheduleWorkOrderRequest(string Technician, DateOnly ScheduledDate);

internal sealed record LogTimeRequest(string Technician, DateOnly Date, int Minutes);

internal sealed record AddMaterialRequest(string Description, decimal Quantity, decimal UnitPrice);
