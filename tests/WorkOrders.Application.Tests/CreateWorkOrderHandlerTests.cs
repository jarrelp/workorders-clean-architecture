using WorkOrders.Application.Tests.Fakes;
using WorkOrders.Application.UseCases;
using WorkOrders.Application.Validation;
using static WorkOrders.Application.Tests.TestData;

namespace WorkOrders.Application.Tests;

public class CreateWorkOrderHandlerTests
{
    private readonly InMemoryWorkOrderRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FixedClock _clock = new(Today);
    private readonly CreateWorkOrderHandler _handler;

    public CreateWorkOrderHandlerTests() =>
        _handler = new CreateWorkOrderHandler(_repository, new SequentialWorkOrderNumberGenerator(), _clock, _unitOfWork);

    [Fact]
    public async Task Create_StoresADraftAndSavesOnce()
    {
        var result = await _handler.HandleAsync(ValidCommand(), Token);

        var response = result.Value;
        Assert.Equal("Draft", response.Status);
        Assert.Equal("WO-2026-0001", response.Number);
        Assert.Equal(72.50m, response.HourlyRate);
        Assert.Equal(new WorkOrderTotalsResponse(0m, 0m, 0m, "EUR"), response.Totals);
        Assert.Equal(response.Id, Assert.Single(_repository.All).Id);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Create_NumbersWithinTheYearOfTheClock()
    {
        var first = await _handler.HandleAsync(ValidCommand(), Token);
        _clock.Today = new DateOnly(2027, 1, 4);
        var second = await _handler.HandleAsync(ValidCommand(), Token);
        var third = await _handler.HandleAsync(ValidCommand(), Token);

        Assert.Equal("WO-2026-0001", first.Value.Number);
        Assert.Equal("WO-2027-0001", second.Value.Number);
        Assert.Equal("WO-2027-0002", third.Value.Number);
    }

    [Fact]
    public async Task Create_WithInvalidFields_ReportsEveryFieldAndSavesNothing()
    {
        var command = new CreateWorkOrderCommand("", " ", "Leaking tap in the staff kitchen", -1m);

        var result = await _handler.HandleAsync(command, Token);

        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal(new[] { "CustomerName", "HourlyRate", "SiteDescription" }, error.FieldErrors.Keys.Order(StringComparer.Ordinal));
        Assert.Empty(_repository.All);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Create_WithTooLongCustomerName_FailsValidation()
    {
        var command = ValidCommand() with { CustomerName = new string('x', FieldLimits.CustomerName + 1) };

        var result = await _handler.HandleAsync(command, Token);

        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal(new[] { "Must be at most 200 characters." }, error.FieldErrors["CustomerName"]);
    }

    private static CreateWorkOrderCommand ValidCommand() => new(
        "Example Installations BV",
        "Office building, third floor",
        "Air conditioning in meeting room 3.04 blows warm air.",
        72.50m);
}
