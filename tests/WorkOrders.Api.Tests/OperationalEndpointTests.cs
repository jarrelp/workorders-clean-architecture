using System.Net;
using System.Text.Json;
using static WorkOrders.Api.Tests.ApiClient;

namespace WorkOrders.Api.Tests;

public class OperationalEndpointTests(WorkOrdersApiFactory factory) : IClassFixture<WorkOrdersApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_ChecksTheDatabaseAndReportsHealthy()
    {
        var response = await _client.GetAsync("/health", Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(Token));
    }

    [Fact]
    public async Task OpenApiDocument_DescribesTheWorkOrderEndpoints()
    {
        var document = await ReadAsync<JsonElement>(await _client.GetAsync("/openapi/v1.json", Token));

        var paths = document.GetProperty("paths").EnumerateObject().Select(path => path.Name).ToList();
        Assert.Contains("/work-orders", paths);
        Assert.Contains("/work-orders/{id}", paths);
        Assert.Contains("/work-orders/{id}/time-entries", paths);
    }
}
