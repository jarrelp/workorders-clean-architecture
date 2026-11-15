using System.Net;
using System.Net.Http.Json;
using WorkOrders.Application.UseCases;
using static WorkOrders.Api.Tests.ApiClient;

namespace WorkOrders.Api.Tests;

public class WorkOrderLifecycleApiTests(WorkOrdersApiFactory factory) : IClassFixture<WorkOrdersApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task WorkOrder_GoesFromDraftToCompleted_AndIsStoredWithItsTotals()
    {
        var response = await _client.PostAsJsonAsync("/work-orders", NewWorkOrder(), Token);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await ReadAsync<WorkOrderResponse>(response);
        Assert.EndsWith($"/work-orders/{created.Id}", response.Headers.Location?.ToString(), StringComparison.Ordinal);
        Assert.Equal("Draft", created.Status);
        Assert.Matches("^WO-2026-[0-9]{4}$", created.Number);

        var id = created.Id;
        await _client.PostOkAsync($"/work-orders/{id}/schedule", new { technician = "Alex Example", scheduledDate = "2026-03-03" });
        await _client.PostOkAsync($"/work-orders/{id}/start");
        await _client.PostOkAsync($"/work-orders/{id}/time-entries", new { technician = "Alex Example", date = "2026-03-03", minutes = 90 });
        await _client.PostOkAsync($"/work-orders/{id}/time-entries", new { technician = "Sam Sample", date = "2026-03-03", minutes = 45 });
        await _client.PostOkAsync($"/work-orders/{id}/materials", new { description = "Thermostatic mixing valve", quantity = 1, unitPrice = 89.95m });
        var completed = await _client.PostOkAsync($"/work-orders/{id}/complete");
        Assert.Equal("Completed", completed.Status);

        // Read it back in a new request, so the assertions are about what was stored.
        var stored = await ReadAsync<WorkOrderResponse>(await _client.GetAsync($"/work-orders/{id}", Token));
        Assert.Equal("Completed", stored.Status);
        Assert.Equal("Alex Example", stored.AssignedTechnician);
        Assert.Equal(new DateOnly(2026, 3, 3), stored.ScheduledDate);
        Assert.Equal(new[] { 90, 45 }, stored.TimeEntries.Select(entry => entry.Minutes));
        Assert.Equal(new MaterialLineResponse("Thermostatic mixing valve", 1m, 89.95m, 89.95m), Assert.Single(stored.MaterialLines));
        Assert.Equal(new WorkOrderTotalsResponse(146.25m, 89.95m, 236.20m, "EUR"), stored.Totals); // 135 min at 65.00 + materials
    }

    [Fact]
    public async Task Cancel_ScheduledWorkOrder_ReturnsTheCancelledWorkOrder()
    {
        var workOrder = await _client.CreateWorkOrderAsync();
        await _client.PostOkAsync($"/work-orders/{workOrder.Id}/schedule", new { technician = "Sam Sample", scheduledDate = "2026-03-02" });

        var cancelled = await _client.PostOkAsync($"/work-orders/{workOrder.Id}/cancel");

        Assert.Equal("Cancelled", cancelled.Status);
    }
}
