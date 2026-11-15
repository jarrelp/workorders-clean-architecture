namespace WorkOrders.Domain.Tests;

public class WorkOrderLifecycleTests
{
    /// <summary>Every (from, to) pair of statuses, and whether the specification allows that move.</summary>
    public static TheoryData<WorkOrderStatus, WorkOrderStatus, bool> AllTransitions()
    {
        var allowed = new HashSet<(WorkOrderStatus, WorkOrderStatus)>
        {
            (WorkOrderStatus.Draft, WorkOrderStatus.Scheduled),
            (WorkOrderStatus.Scheduled, WorkOrderStatus.InProgress),
            (WorkOrderStatus.InProgress, WorkOrderStatus.Completed),
            (WorkOrderStatus.Draft, WorkOrderStatus.Cancelled),
            (WorkOrderStatus.Scheduled, WorkOrderStatus.Cancelled),
            (WorkOrderStatus.InProgress, WorkOrderStatus.Cancelled),
        };

        var data = new TheoryData<WorkOrderStatus, WorkOrderStatus, bool>();
        foreach (var from in Enum.GetValues<WorkOrderStatus>())
        {
            foreach (var to in Enum.GetValues<WorkOrderStatus>())
            {
                data.Add(from, to, allowed.Contains((from, to)));
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(AllTransitions))]
    public void CanTransitionTo_AllowsOnlyTheSpecifiedTransitions(WorkOrderStatus from, WorkOrderStatus to, bool expected)
    {
        Assert.Equal(expected, from.CanTransitionTo(to));
    }

    [Theory]
    [InlineData(WorkOrderStatus.Draft, false)]
    [InlineData(WorkOrderStatus.Scheduled, false)]
    [InlineData(WorkOrderStatus.InProgress, false)]
    [InlineData(WorkOrderStatus.Completed, true)]
    [InlineData(WorkOrderStatus.Cancelled, true)]
    public void IsClosed_IsTrueOnlyForCompletedAndCancelled(WorkOrderStatus status, bool expected)
    {
        Assert.Equal(expected, status.IsClosed());
    }
}
