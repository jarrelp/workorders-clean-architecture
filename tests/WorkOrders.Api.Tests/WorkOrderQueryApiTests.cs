using System.Net;
using WorkOrders.Application.UseCases;
using static WorkOrders.Api.Tests.ApiClient;

namespace WorkOrders.Api.Tests;

public class WorkOrderQueryApiTests(WorkOrdersApiFactory factory) : IClassFixture<WorkOrdersApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task List_WithoutStatus_ReturnsWorkOrdersOrderedByNumber()
    {
        var first = await _client.CreateWorkOrderAsync(NewWorkOrder("Example Housing Association"));
        var second = await _client.CreateWorkOrderAsync(NewWorkOrder("Example Bakery"));

        var summaries = await ReadAsync<List<WorkOrderSummaryResponse>>(await _client.GetAsync("/work-orders", Token));

        var numbers = summaries.Select(summary => summary.Number).ToList();
        Assert.Equal(numbers.Order(StringComparer.Ordinal), numbers);
        Assert.True(numbers.IndexOf(first.Number) < numbers.IndexOf(second.Number));
    }

    [Fact]
    public async Task List_WithStatus_ReturnsOnlyWorkOrdersInThatStatus()
    {
        var draft = await _client.CreateWorkOrderAsync();
        var scheduled = await _client.CreateWorkOrderAsync();
        await _client.PostOkAsync($"/work-orders/{scheduled.Id}/schedule", new { technician = "Alex Example", scheduledDate = "2026-03-02" });

        var summaries = await ReadAsync<List<WorkOrderSummaryResponse>>(await _client.GetAsync("/work-orders?status=scheduled", Token));

        Assert.Contains(summaries, summary => summary.Id == scheduled.Id);
        Assert.DoesNotContain(summaries, summary => summary.Id == draft.Id);
        Assert.All(summaries, summary => Assert.Equal("Scheduled", summary.Status));
    }

    [Fact]
    public async Task List_WithUnknownStatus_Returns400WithAFieldError()
    {
        var response = await _client.GetAsync("/work-orders?status=Open", Token);

        var problem = await AssertProblemAsync(response, HttpStatusCode.BadRequest, "Request.Invalid");
        Assert.True(problem.GetProperty("errors").TryGetProperty("status", out _));
    }

    [Fact]
    public async Task Get_UnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/work-orders/{Guid.NewGuid()}", Token);

        await AssertProblemAsync(response, HttpStatusCode.NotFound, "WorkOrder.NotFound");
    }
}
