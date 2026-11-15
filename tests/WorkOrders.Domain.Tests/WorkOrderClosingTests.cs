using WorkOrders.Domain.Common;
using static WorkOrders.Domain.Tests.TestWorkOrders;

namespace WorkOrders.Domain.Tests;

/// <summary>Completing and cancelling, and what a closed work order still allows (nothing).</summary>
public class WorkOrderClosingTests
{
    private static readonly string[] Changes = ["Schedule", "Start", "LogTime", "AddMaterial", "Complete", "Cancel"];

    public static TheoryData<WorkOrderStatus, string> ChangesToClosedWorkOrders()
    {
        var data = new TheoryData<WorkOrderStatus, string>();
        foreach (var status in new[] { WorkOrderStatus.Completed, WorkOrderStatus.Cancelled })
        {
            foreach (var change in Changes)
            {
                data.Add(status, change);
            }
        }

        return data;
    }

    [Fact]
    public void Complete_WithTimeLogged_MovesToCompleted()
    {
        var workOrder = InProgress();
        ResultAssert.Succeeded(workOrder.LogTime(Technician, Today, 45));

        var result = workOrder.Complete();

        ResultAssert.Succeeded(result);
        Assert.Equal(WorkOrderStatus.Completed, workOrder.Status);
    }

    [Fact]
    public void Complete_WithoutTimeEntries_Fails()
    {
        var workOrder = InProgress();

        var result = workOrder.Complete();

        ResultAssert.Failed(WorkOrderErrors.NoTimeLogged, result);
        Assert.Equal(WorkOrderStatus.InProgress, workOrder.Status);
    }

    [Fact]
    public void Complete_BeforeTheWorkStarted_IsAnInvalidTransition()
    {
        var workOrder = Scheduled();

        var result = workOrder.Complete();

        ResultAssert.Failed(WorkOrderErrors.InvalidTransition(WorkOrderStatus.Scheduled, WorkOrderStatus.Completed), result);
    }

    [Theory]
    [InlineData(WorkOrderStatus.Draft)]
    [InlineData(WorkOrderStatus.Scheduled)]
    [InlineData(WorkOrderStatus.InProgress)]
    public void Cancel_BeforeCompletion_IsAllowed(WorkOrderStatus status)
    {
        var workOrder = InStatus(status);

        var result = workOrder.Cancel();

        ResultAssert.Succeeded(result);
        Assert.Equal(WorkOrderStatus.Cancelled, workOrder.Status);
    }

    [Theory]
    [MemberData(nameof(ChangesToClosedWorkOrders))]
    public void ClosedWorkOrder_RejectsEveryChange(WorkOrderStatus closedStatus, string change)
    {
        var workOrder = InStatus(closedStatus);
        var timeEntriesBefore = workOrder.TimeEntries.Count;
        var materialLinesBefore = workOrder.MaterialLines.Count;

        var result = Apply(change, workOrder);

        ResultAssert.Failed(WorkOrderErrors.Closed(closedStatus), result);
        Assert.Equal(closedStatus, workOrder.Status);
        Assert.Equal(timeEntriesBefore, workOrder.TimeEntries.Count);
        Assert.Equal(materialLinesBefore, workOrder.MaterialLines.Count);
    }

    private static Result Apply(string change, WorkOrder workOrder) => change switch
    {
        "Schedule" => workOrder.Schedule("Sam Sample", Today.AddDays(1), Today),
        "Start" => workOrder.Start(),
        "LogTime" => workOrder.LogTime("Sam Sample", Today, 30),
        "AddMaterial" => workOrder.AddMaterial("Sealing ring", 1m, Euros(2.50m)),
        "Complete" => workOrder.Complete(),
        "Cancel" => workOrder.Cancel(),
        _ => throw new ArgumentOutOfRangeException(nameof(change), change, "Unknown change."),
    };
}
