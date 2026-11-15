using static WorkOrders.Domain.Tests.TestWorkOrders;

namespace WorkOrders.Domain.Tests;

/// <summary>Starting the work, then logging time and materials while it is in progress.</summary>
public class WorkOrderExecutionTests
{
    [Fact]
    public void Start_FromScheduled_MovesToInProgress()
    {
        var workOrder = Scheduled();

        var result = workOrder.Start();

        ResultAssert.Succeeded(result);
        Assert.Equal(WorkOrderStatus.InProgress, workOrder.Status);
    }

    [Fact]
    public void Start_FromDraft_IsAnInvalidTransition()
    {
        var workOrder = Draft();

        var result = workOrder.Start();

        ResultAssert.Failed(WorkOrderErrors.InvalidTransition(WorkOrderStatus.Draft, WorkOrderStatus.InProgress), result);
        Assert.Equal(WorkOrderStatus.Draft, workOrder.Status);
    }

    [Fact]
    public void LogTime_WhileInProgress_AddsAnEntry()
    {
        var workOrder = InProgress();

        var result = workOrder.LogTime(" Sam Sample ", Today, 90);

        ResultAssert.Succeeded(result);
        var entry = Assert.Single(workOrder.TimeEntries);
        Assert.Equal("Sam Sample", entry.Technician);
        Assert.Equal(Today, entry.Date);
        Assert.Equal(90, entry.Minutes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    public void LogTime_WithoutPositiveMinutes_Fails(int minutes)
    {
        var workOrder = InProgress();

        var result = workOrder.LogTime(Technician, Today, minutes);

        ResultAssert.Failed(WorkOrderErrors.MinutesMustBePositive, result);
        Assert.Empty(workOrder.TimeEntries);
    }

    [Fact]
    public void LogTime_WithoutTechnician_Fails()
    {
        var workOrder = InProgress();

        var result = workOrder.LogTime(" ", Today, 30);

        ResultAssert.Failed(WorkOrderErrors.TechnicianRequired, result);
    }

    [Fact]
    public void LogTime_BeforeTheWorkStarted_Fails()
    {
        var workOrder = Scheduled();

        var result = workOrder.LogTime(Technician, Today, 30);

        ResultAssert.Failed(WorkOrderErrors.NotInProgress(WorkOrderStatus.Scheduled), result);
        Assert.Empty(workOrder.TimeEntries);
    }

    [Fact]
    public void AddMaterial_WhileInProgress_AddsALine()
    {
        var workOrder = InProgress();

        var result = workOrder.AddMaterial("Pressure relief valve 3 bar", 2m, Euros(24.95m));

        ResultAssert.Succeeded(result);
        var line = Assert.Single(workOrder.MaterialLines);
        Assert.Equal("Pressure relief valve 3 bar", line.Description);
        Assert.Equal(2m, line.Quantity);
        Assert.Equal(Euros(49.90m), line.LineTotal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddMaterial_WithoutPositiveQuantity_Fails(int quantity)
    {
        var workOrder = InProgress();

        var result = workOrder.AddMaterial("Sealing ring", quantity, Euros(2.50m));

        ResultAssert.Failed(WorkOrderErrors.QuantityMustBePositive, result);
        Assert.Empty(workOrder.MaterialLines);
    }

    [Fact]
    public void AddMaterial_BeforeTheWorkStarted_Fails()
    {
        var workOrder = Draft();

        var result = workOrder.AddMaterial("Sealing ring", 1m, Euros(2.50m));

        ResultAssert.Failed(WorkOrderErrors.NotInProgress(WorkOrderStatus.Draft), result);
        Assert.Empty(workOrder.MaterialLines);
    }
}
