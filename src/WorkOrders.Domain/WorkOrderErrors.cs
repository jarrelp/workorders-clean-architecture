using System.Globalization;
using WorkOrders.Domain.Common;

namespace WorkOrders.Domain;

/// <summary>Every expected failure of a work order, with a stable code that clients can rely on.</summary>
public static class WorkOrderErrors
{
    public static readonly Error CustomerNameRequired =
        Error.Validation("WorkOrder.CustomerNameRequired", "A customer name is required.");

    public static readonly Error SiteDescriptionRequired =
        Error.Validation("WorkOrder.SiteDescriptionRequired", "A site description is required.");

    public static readonly Error ProblemDescriptionRequired =
        Error.Validation("WorkOrder.ProblemDescriptionRequired", "A problem description is required.");

    public static readonly Error TechnicianRequired =
        Error.Validation("WorkOrder.TechnicianRequired", "A technician is required.");

    public static readonly Error MinutesMustBePositive =
        Error.Validation("WorkOrder.MinutesMustBePositive", "Logged time must be more than zero minutes.");

    public static readonly Error MaterialDescriptionRequired =
        Error.Validation("WorkOrder.MaterialDescriptionRequired", "A material description is required.");

    public static readonly Error QuantityMustBePositive =
        Error.Validation("WorkOrder.QuantityMustBePositive", "The quantity must be greater than zero.");

    public static readonly Error NoTimeLogged =
        Error.Conflict("WorkOrder.NoTimeLogged", "A work order needs at least one time entry before it can be completed.");

    public static Error ScheduledDateInPast(DateOnly scheduledDate, DateOnly today) =>
        Error.Validation(
            "WorkOrder.ScheduledDateInPast",
            string.Create(CultureInfo.InvariantCulture, $"The scheduled date {scheduledDate:yyyy-MM-dd} is before today ({today:yyyy-MM-dd})."));

    public static Error InvalidTransition(WorkOrderStatus from, WorkOrderStatus to) =>
        Error.Conflict("WorkOrder.InvalidStatusTransition", $"A work order cannot move from {from} to {to}.");

    public static Error NotInProgress(WorkOrderStatus status) =>
        Error.Conflict(
            "WorkOrder.NotInProgress",
            $"Time and materials can only be added while the work order is in progress. It is {status}.");

    public static Error Closed(WorkOrderStatus status) =>
        Error.Conflict("WorkOrder.Closed", $"The work order is {status} and can no longer be changed.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("WorkOrder.NotFound", $"Work order '{id}' does not exist.");
}
