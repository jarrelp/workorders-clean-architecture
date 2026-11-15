using WorkOrders.Application.Tests.Fakes;
using WorkOrders.Application.UseCases;
using WorkOrders.Application.Validation;
using WorkOrders.Domain;
using static WorkOrders.Application.Tests.TestData;

namespace WorkOrders.Application.Tests;

/// <summary>Reading work orders: one by id, or a list filtered by status.</summary>
public class QueryHandlerTests
{
    private readonly InMemoryWorkOrderRepository _repository = new();

    [Fact]
    public async Task Get_ReturnsTheWorkOrderWithLinesAndTotals()
    {
        var workOrder = _repository.Seed(WorkOrderStatus.InProgress);
        Succeed(workOrder.LogTime(Technician, Today, 30));
        Succeed(workOrder.AddMaterial("Start capacitor for door motor", 1m, Euros(18.50m)));

        var result = await new GetWorkOrderHandler(_repository).HandleAsync(new GetWorkOrderQuery(workOrder.Id), Token);

        var response = result.Value;
        Assert.Equal(workOrder.Number.Value, response.Number);
        Assert.Equal("InProgress", response.Status);
        Assert.Equal(new TimeEntryResponse(Technician, Today, 30), Assert.Single(response.TimeEntries));
        Assert.Equal(new MaterialLineResponse("Start capacitor for door motor", 1m, 18.50m, 18.50m), Assert.Single(response.MaterialLines));
        Assert.Equal(new WorkOrderTotalsResponse(30.00m, 18.50m, 48.50m, "EUR"), response.Totals);
    }

    [Fact]
    public async Task Get_UnknownId_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        var result = await new GetWorkOrderHandler(_repository).HandleAsync(new GetWorkOrderQuery(id), Token);

        Assert.Equal(WorkOrderErrors.NotFound(id), result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task List_WithoutStatus_ReturnsEveryWorkOrder(string? status)
    {
        _repository.Seed(WorkOrderStatus.Draft);
        _repository.Seed(WorkOrderStatus.Scheduled);
        _repository.Seed(WorkOrderStatus.Cancelled);

        var result = await new ListWorkOrdersHandler(_repository).HandleAsync(new ListWorkOrdersQuery(status), Token);

        Assert.Equal(new[] { "WO-2026-0001", "WO-2026-0002", "WO-2026-0003" }, result.Value.Select(summary => summary.Number));
    }

    [Theory]
    [InlineData("Scheduled")]
    [InlineData("scheduled")]
    [InlineData(" SCHEDULED ")]
    public async Task List_WithStatus_IgnoresCaseAndFilters(string status)
    {
        _repository.Seed(WorkOrderStatus.Draft);
        var scheduled = _repository.Seed(WorkOrderStatus.Scheduled);

        var result = await new ListWorkOrdersHandler(_repository).HandleAsync(new ListWorkOrdersQuery(status), Token);

        var summary = Assert.Single(result.Value);
        Assert.Equal(scheduled.Id, summary.Id);
        Assert.Equal("Scheduled", summary.Status);
    }

    [Theory]
    [InlineData("Open")]
    [InlineData("1")]
    public async Task List_WithUnknownStatus_IsAValidationErrorOnStatus(string status)
    {
        var result = await new ListWorkOrdersHandler(_repository).HandleAsync(new ListWorkOrdersQuery(status), Token);

        var error = Assert.IsType<ValidationError>(result.Error);
        var message = Assert.Single(error.FieldErrors["Status"]);
        Assert.Equal("Must be one of: Draft, Scheduled, InProgress, Completed, Cancelled.", message);
    }
}
