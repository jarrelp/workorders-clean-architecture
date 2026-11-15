using static WorkOrders.Domain.Tests.TestWorkOrders;

namespace WorkOrders.Domain.Tests;

public class WorkOrderSchedulingTests
{
    [Fact]
    public void Schedule_AssignsTechnicianAndDate()
    {
        var workOrder = Draft();

        var result = workOrder.Schedule("Sam Sample", Today.AddDays(3), Today);

        ResultAssert.Succeeded(result);
        Assert.Equal(WorkOrderStatus.Scheduled, workOrder.Status);
        Assert.Equal("Sam Sample", workOrder.AssignedTechnician);
        Assert.Equal(Today.AddDays(3), workOrder.ScheduledDate);
    }

    [Fact]
    public void Schedule_ForToday_IsAllowed()
    {
        var workOrder = Draft();

        var result = workOrder.Schedule(Technician, Today, Today);

        ResultAssert.Succeeded(result);
        Assert.Equal(Today, workOrder.ScheduledDate);
    }

    [Fact]
    public void Schedule_ForADateInThePast_Fails()
    {
        var workOrder = Draft();
        var yesterday = Today.AddDays(-1);

        var result = workOrder.Schedule(Technician, yesterday, Today);

        ResultAssert.Failed(WorkOrderErrors.ScheduledDateInPast(yesterday, Today), result);
        Assert.Equal(WorkOrderStatus.Draft, workOrder.Status);
        Assert.Null(workOrder.ScheduledDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Schedule_WithoutTechnician_Fails(string technician)
    {
        var workOrder = Draft();

        var result = workOrder.Schedule(technician, Today.AddDays(1), Today);

        ResultAssert.Failed(WorkOrderErrors.TechnicianRequired, result);
        Assert.Equal(WorkOrderStatus.Draft, workOrder.Status);
        Assert.Null(workOrder.AssignedTechnician);
    }

    [Fact]
    public void Schedule_WhenAlreadyScheduled_IsAnInvalidTransition()
    {
        var workOrder = Scheduled();

        var result = workOrder.Schedule("Sam Sample", Today.AddDays(5), Today);

        ResultAssert.Failed(WorkOrderErrors.InvalidTransition(WorkOrderStatus.Scheduled, WorkOrderStatus.Scheduled), result);
        Assert.Equal(Technician, workOrder.AssignedTechnician);
    }
}
