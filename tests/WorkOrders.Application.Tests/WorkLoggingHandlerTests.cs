using WorkOrders.Application.Tests.Fakes;
using WorkOrders.Application.UseCases;
using WorkOrders.Application.Validation;
using WorkOrders.Domain;
using static WorkOrders.Application.Tests.TestData;

namespace WorkOrders.Application.Tests;

/// <summary>Logging time and materials on a work order that is in progress.</summary>
public class WorkLoggingHandlerTests
{
    private readonly InMemoryWorkOrderRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    [Fact]
    public async Task LogTime_AddsTheEntryAndUpdatesTheLabourTotal()
    {
        var workOrder = _repository.Seed(WorkOrderStatus.InProgress);
        var command = new LogTimeCommand(workOrder.Id, Technician, Today, 90);

        var result = await new LogTimeHandler(_repository, _unitOfWork).HandleAsync(command, Token);

        Assert.Equal(new TimeEntryResponse(Technician, Today, 90), Assert.Single(result.Value.TimeEntries));
        Assert.Equal(90.00m, result.Value.Totals.Labour); // 1.5 hours at 60.00
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task LogTime_WithInvalidFields_ReportsEveryField()
    {
        var workOrder = _repository.Seed(WorkOrderStatus.InProgress);
        var command = new LogTimeCommand(workOrder.Id, "", Today, 0);

        var result = await new LogTimeHandler(_repository, _unitOfWork).HandleAsync(command, Token);

        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal(new[] { "Minutes", "Technician" }, error.FieldErrors.Keys.Order(StringComparer.Ordinal));
        Assert.Empty(workOrder.TimeEntries);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task LogTime_OnACompletedWorkOrder_IsRejected()
    {
        var workOrder = _repository.Seed(WorkOrderStatus.Completed);
        var command = new LogTimeCommand(workOrder.Id, Technician, Today, 15);

        var result = await new LogTimeHandler(_repository, _unitOfWork).HandleAsync(command, Token);

        Assert.Equal(WorkOrderErrors.Closed(WorkOrderStatus.Completed), result.Error);
        Assert.Single(workOrder.TimeEntries);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task AddMaterial_AddsTheLineAndUpdatesTheTotals()
    {
        var workOrder = _repository.Seed(WorkOrderStatus.InProgress);
        var command = new AddMaterialCommand(workOrder.Id, "Limit switch for roller door", 2m, 14.95m);

        var result = await new AddMaterialHandler(_repository, _unitOfWork).HandleAsync(command, Token);

        Assert.Equal(new MaterialLineResponse("Limit switch for roller door", 2m, 14.95m, 29.90m), Assert.Single(result.Value.MaterialLines));
        Assert.Equal(new WorkOrderTotalsResponse(0m, 29.90m, 29.90m, "EUR"), result.Value.Totals);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task AddMaterial_WithInvalidFields_ReportsEveryField()
    {
        var workOrder = _repository.Seed(WorkOrderStatus.InProgress);
        var command = new AddMaterialCommand(workOrder.Id, " ", 0m, -2.50m);

        var result = await new AddMaterialHandler(_repository, _unitOfWork).HandleAsync(command, Token);

        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal(new[] { "Description", "Quantity", "UnitPrice" }, error.FieldErrors.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task AddMaterial_BeforeTheWorkStarted_IsAConflict()
    {
        var workOrder = _repository.Seed(WorkOrderStatus.Scheduled);
        var command = new AddMaterialCommand(workOrder.Id, "Limit switch for roller door", 1m, 14.95m);

        var result = await new AddMaterialHandler(_repository, _unitOfWork).HandleAsync(command, Token);

        Assert.Equal(WorkOrderErrors.NotInProgress(WorkOrderStatus.Scheduled), result.Error);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
