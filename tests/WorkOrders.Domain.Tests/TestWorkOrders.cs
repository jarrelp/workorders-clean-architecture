using WorkOrders.Domain.Common;

namespace WorkOrders.Domain.Tests;

/// <summary>Work orders in a known state, so each test only spells out what matters to it.</summary>
internal static class TestWorkOrders
{
    public const string Technician = "Alex Example";

    public static readonly DateOnly Today = new(2026, 3, 2);

    public static Money Euros(decimal amount) => Money.FromEuros(amount).Value;

    public static WorkOrder Draft(decimal hourlyRate = 65m) =>
        WorkOrder.Create(
            WorkOrderNumber.Create(2026, 1),
            "Example Housing Association",
            "Boiler room, basement of building B",
            "Central heating boiler shows error E119 and does not start.",
            Euros(hourlyRate)).Value;

    public static WorkOrder Scheduled(decimal hourlyRate = 65m)
    {
        var workOrder = Draft(hourlyRate);
        ResultAssert.Succeeded(workOrder.Schedule(Technician, Today.AddDays(1), Today));
        return workOrder;
    }

    public static WorkOrder InProgress(decimal hourlyRate = 65m)
    {
        var workOrder = Scheduled(hourlyRate);
        ResultAssert.Succeeded(workOrder.Start());
        return workOrder;
    }

    public static WorkOrder Completed()
    {
        var workOrder = InProgress();
        ResultAssert.Succeeded(workOrder.LogTime(Technician, Today.AddDays(1), 60));
        ResultAssert.Succeeded(workOrder.Complete());
        return workOrder;
    }

    public static WorkOrder Cancelled()
    {
        var workOrder = Draft();
        ResultAssert.Succeeded(workOrder.Cancel());
        return workOrder;
    }

    public static WorkOrder InStatus(WorkOrderStatus status) => status switch
    {
        WorkOrderStatus.Draft => Draft(),
        WorkOrderStatus.Scheduled => Scheduled(),
        WorkOrderStatus.InProgress => InProgress(),
        WorkOrderStatus.Completed => Completed(),
        WorkOrderStatus.Cancelled => Cancelled(),
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown status."),
    };
}
