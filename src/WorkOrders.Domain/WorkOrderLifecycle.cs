namespace WorkOrders.Domain;

/// <summary>
/// The status transitions of a work order, kept in one place:
/// Draft → Scheduled → InProgress → Completed, and Cancelled from any status before Completed.
/// </summary>
public static class WorkOrderLifecycle
{
    public static bool CanTransitionTo(this WorkOrderStatus current, WorkOrderStatus next) => (current, next) switch
    {
        (WorkOrderStatus.Draft, WorkOrderStatus.Scheduled) => true,
        (WorkOrderStatus.Scheduled, WorkOrderStatus.InProgress) => true,
        (WorkOrderStatus.InProgress, WorkOrderStatus.Completed) => true,
        (WorkOrderStatus.Draft or WorkOrderStatus.Scheduled or WorkOrderStatus.InProgress, WorkOrderStatus.Cancelled) => true,
        _ => false,
    };

    /// <summary>A completed or cancelled work order can no longer change.</summary>
    public static bool IsClosed(this WorkOrderStatus status) =>
        status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled;
}
