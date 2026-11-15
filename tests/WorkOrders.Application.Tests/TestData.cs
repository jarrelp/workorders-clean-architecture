using WorkOrders.Application.Tests.Fakes;
using WorkOrders.Domain;
using WorkOrders.Domain.Common;

namespace WorkOrders.Application.Tests;

internal static class TestData
{
    public const string Technician = "Alex Example";

    public static readonly DateOnly Today = new(2026, 3, 2);

    /// <summary>Cancelled when xUnit stops the test run early.</summary>
    public static CancellationToken Token => TestContext.Current.CancellationToken;

    public static Money Euros(decimal amount) => Money.FromEuros(amount).Value;

    public static void Succeed(Result result) =>
        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.Description : null);

    /// <summary>Stores a work order that went through the normal steps up to <paramref name="status"/>.</summary>
    public static WorkOrder Seed(this InMemoryWorkOrderRepository repository, WorkOrderStatus status)
    {
        var workOrder = WorkOrder.Create(
            WorkOrderNumber.Create(Today.Year, repository.All.Count + 1),
            "Example Installations BV",
            "Warehouse, loading dock 2",
            "Roller door motor trips the circuit breaker.",
            Euros(60m)).Value;

        if (status is WorkOrderStatus.Scheduled or WorkOrderStatus.InProgress or WorkOrderStatus.Completed)
        {
            Succeed(workOrder.Schedule(Technician, Today.AddDays(1), Today));
        }

        if (status is WorkOrderStatus.InProgress or WorkOrderStatus.Completed)
        {
            Succeed(workOrder.Start());
        }

        if (status is WorkOrderStatus.Completed)
        {
            Succeed(workOrder.LogTime(Technician, Today.AddDays(1), 60));
            Succeed(workOrder.Complete());
        }

        if (status is WorkOrderStatus.Cancelled)
        {
            Succeed(workOrder.Cancel());
        }

        repository.Add(workOrder);
        return workOrder;
    }
}
