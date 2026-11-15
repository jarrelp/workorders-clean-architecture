using WorkOrders.Application.Tests.Fakes;
using WorkOrders.Application.UseCases;
using WorkOrders.Application.Validation;
using WorkOrders.Domain;
using static WorkOrders.Application.Tests.TestData;

namespace WorkOrders.Application.Tests;

/// <summary>The use cases that move a work order through its statuses.</summary>
public class StatusHandlerTests
{
    private readonly InMemoryWorkOrderRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FixedClock _clock = new(Today);

    [Fact]
    public async Task Schedule_AssignsTechnicianAndDateAndSaves()
    {
        var workOrder = _repository.Seed(WorkOrderStatus.Draft);
        var command = new ScheduleWorkOrderCommand(workOrder.Id, "Sam Sample", Today.AddDays(2));

        var result = await new ScheduleWorkOrderHandler(_repository, _clock, _unitOfWork).HandleAsync(command, Token);

        Assert.Equal("Scheduled", result.Value.Status);
        Assert.Equal("Sam Sample", result.Value.AssignedTechnician);
        Assert.Equal(Today.AddDays(2), result.Value.ScheduledDate);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Schedule_UsesTheInjectedClockForToday()
    {
        var workOrder = _repository.Seed(WorkOrderStatus.Draft);
        var visit = new DateOnly(2026, 6, 1);
        _clock.Today = visit.AddDays(1);

        var command = new ScheduleWorkOrderCommand(workOrder.Id, "Sam Sample", visit);
        var result = await new ScheduleWorkOrderHandler(_repository, _clock, _unitOfWork).HandleAsync(command, Token);

        Assert.Equal(WorkOrderErrors.ScheduledDateInPast(visit, _clock.Today), result.Error);
        Assert.Equal(WorkOrderStatus.Draft, workOrder.Status);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Schedule_WithoutTechnician_IsAValidationError()
    {
        var workOrder = _repository.Seed(WorkOrderStatus.Draft);
        var command = new ScheduleWorkOrderCommand(workOrder.Id, " ", Today);

        var result = await new ScheduleWorkOrderHandler(_repository, _clock, _unitOfWork).HandleAsync(command, Token);

        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal(new[] { "Technician" }, error.FieldErrors.Keys);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Schedule_UnknownWorkOrder_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var command = new ScheduleWorkOrderCommand(id, "Sam Sample", Today);

        var result = await new ScheduleWorkOrderHandler(_repository, _clock, _unitOfWork).HandleAsync(command, Token);

        Assert.Equal(WorkOrderErrors.NotFound(id), result.Error);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Start_ScheduledWorkOrder_IsInProgressAndSaved()
    {
        var workOrder = _repository.Seed(WorkOrderStatus.Scheduled);

        var result = await new StartWorkOrderHandler(_repository, _unitOfWork).HandleAsync(new StartWorkOrderCommand(workOrder.Id), Token);

        Assert.Equal("InProgress", result.Value.Status);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Start_DraftWorkOrder_IsAnInvalidTransitionAndSavesNothing()
    {
        var workOrder = _repository.Seed(WorkOrderStatus.Draft);

        var result = await new StartWorkOrderHandler(_repository, _unitOfWork).HandleAsync(new StartWorkOrderCommand(workOrder.Id), Token);

        Assert.Equal(WorkOrderErrors.InvalidTransition(WorkOrderStatus.Draft, WorkOrderStatus.InProgress), result.Error);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Complete_WithTimeLogged_IsCompletedAndSaved()
    {
        var workOrder = _repository.Seed(WorkOrderStatus.InProgress);
        Succeed(workOrder.LogTime(Technician, Today, 45));

        var result = await new CompleteWorkOrderHandler(_repository, _unitOfWork).HandleAsync(new CompleteWorkOrderCommand(workOrder.Id), Token);

        Assert.Equal("Completed", result.Value.Status);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Complete_WithoutTimeLogged_IsAConflictAndSavesNothing()
    {
        var workOrder = _repository.Seed(WorkOrderStatus.InProgress);

        var result = await new CompleteWorkOrderHandler(_repository, _unitOfWork).HandleAsync(new CompleteWorkOrderCommand(workOrder.Id), Token);

        Assert.Equal(WorkOrderErrors.NoTimeLogged, result.Error);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Cancel_InProgressWorkOrder_IsCancelledAndSaved()
    {
        var workOrder = _repository.Seed(WorkOrderStatus.InProgress);

        var result = await new CancelWorkOrderHandler(_repository, _unitOfWork).HandleAsync(new CancelWorkOrderCommand(workOrder.Id), Token);

        Assert.Equal("Cancelled", result.Value.Status);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Cancel_CompletedWorkOrder_IsRejectedBecauseItIsClosed()
    {
        var workOrder = _repository.Seed(WorkOrderStatus.Completed);

        var result = await new CancelWorkOrderHandler(_repository, _unitOfWork).HandleAsync(new CancelWorkOrderCommand(workOrder.Id), Token);

        Assert.Equal(WorkOrderErrors.Closed(WorkOrderStatus.Completed), result.Error);
        Assert.Equal(WorkOrderStatus.Completed, workOrder.Status);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
