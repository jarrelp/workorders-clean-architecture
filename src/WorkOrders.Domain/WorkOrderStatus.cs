namespace WorkOrders.Domain;

/// <summary>Where a work order is in its lifecycle. <see cref="WorkOrderLifecycle"/> defines the allowed moves.</summary>
public enum WorkOrderStatus
{
    Draft,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}
