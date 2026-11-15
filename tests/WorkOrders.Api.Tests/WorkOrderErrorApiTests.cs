using System.Net;
using System.Net.Http.Json;
using System.Text;
using static WorkOrders.Api.Tests.ApiClient;

namespace WorkOrders.Api.Tests;

/// <summary>Expected failures map to RFC 9457 problem details: 400 validation, 404 not found, 409 invalid transition.</summary>
public class WorkOrderErrorApiTests(WorkOrdersApiFactory factory) : IClassFixture<WorkOrdersApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_WithInvalidFields_Returns400WithAnErrorPerField()
    {
        var body = new { customerName = "", siteDescription = " ", problemDescription = "Leaking tap", hourlyRate = -5 };

        var response = await _client.PostAsJsonAsync("/work-orders", body, Token);

        var problem = await AssertProblemAsync(response, HttpStatusCode.BadRequest, "Request.Invalid");
        var fields = problem.GetProperty("errors").EnumerateObject().Select(field => field.Name).Order(StringComparer.Ordinal);
        Assert.Equal(new[] { "customerName", "hourlyRate", "siteDescription" }, fields);
        Assert.Equal("/work-orders", problem.GetProperty("instance").GetString());
    }

    [Theory]
    [InlineData("{ this is not json")]
    [InlineData("""{ "customerName": "Example Installations BV" }""")]
    public async Task Create_WithMalformedOrIncompleteJson_Returns400(string json)
    {
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/work-orders", content, Token);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Schedule_ForADateInThePast_Returns400()
    {
        var workOrder = await _client.CreateWorkOrderAsync();
        var body = new { technician = "Alex Example", scheduledDate = "2026-03-01" }; // the factory's clock says 2026-03-02

        var response = await _client.PostAsJsonAsync($"/work-orders/{workOrder.Id}/schedule", body, Token);

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "WorkOrder.ScheduledDateInPast");
    }

    [Fact]
    public async Task Start_UnknownWorkOrder_Returns404()
    {
        var response = await _client.PostAsync($"/work-orders/{Guid.NewGuid()}/start", content: null, Token);

        await AssertProblemAsync(response, HttpStatusCode.NotFound, "WorkOrder.NotFound");
    }

    [Fact]
    public async Task Start_DraftWorkOrder_Returns409()
    {
        var workOrder = await _client.CreateWorkOrderAsync();

        var response = await _client.PostAsync($"/work-orders/{workOrder.Id}/start", content: null, Token);

        await AssertProblemAsync(response, HttpStatusCode.Conflict, "WorkOrder.InvalidStatusTransition");
    }

    [Fact]
    public async Task Complete_WithoutTimeEntries_Returns409()
    {
        var workOrder = await _client.CreateWorkOrderAsync();
        await _client.PostOkAsync($"/work-orders/{workOrder.Id}/schedule", new { technician = "Alex Example", scheduledDate = "2026-03-02" });
        await _client.PostOkAsync($"/work-orders/{workOrder.Id}/start");

        var response = await _client.PostAsync($"/work-orders/{workOrder.Id}/complete", content: null, Token);

        await AssertProblemAsync(response, HttpStatusCode.Conflict, "WorkOrder.NoTimeLogged");
    }

    [Fact]
    public async Task LogTime_OnACancelledWorkOrder_Returns409()
    {
        var workOrder = await _client.CreateWorkOrderAsync();
        await _client.PostOkAsync($"/work-orders/{workOrder.Id}/cancel");
        var body = new { technician = "Alex Example", date = "2026-03-02", minutes = 30 };

        var response = await _client.PostAsJsonAsync($"/work-orders/{workOrder.Id}/time-entries", body, Token);

        await AssertProblemAsync(response, HttpStatusCode.Conflict, "WorkOrder.Closed");
    }
}
