using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WorkOrders.Application.UseCases;

namespace WorkOrders.Api.Tests;

/// <summary>Small helpers that keep the tests about behaviour instead of HTTP plumbing.</summary>
internal static class ApiClient
{
    public static CancellationToken Token => TestContext.Current.CancellationToken;

    public static object NewWorkOrder(string customerName = "Example Installations BV") => new
    {
        customerName,
        siteDescription = "Sports hall, changing room 2",
        problemDescription = "Shower thermostat delivers cold water only.",
        hourlyRate = 65.00m,
    };

    public static async Task<WorkOrderResponse> CreateWorkOrderAsync(this HttpClient client, object? body = null)
    {
        var response = await client.PostAsJsonAsync("/work-orders", body ?? NewWorkOrder(), Token);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadAsync<WorkOrderResponse>(response);
    }

    /// <summary>Sends a command that must succeed, and returns the updated work order.</summary>
    public static async Task<WorkOrderResponse> PostOkAsync(this HttpClient client, string uri, object? body = null)
    {
        var response = body is null
            ? await client.PostAsync(uri, content: null, Token)
            : await client.PostAsJsonAsync(uri, body, Token);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await ReadAsync<WorkOrderResponse>(response);
    }

    public static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        var value = await response.Content.ReadFromJsonAsync<T>(Token);
        Assert.NotNull(value);
        return value;
    }

    /// <summary>Asserts an RFC 9457 response with the given status and error code, and returns its body.</summary>
    public static async Task<JsonElement> AssertProblemAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await ReadAsync<JsonElement>(response);
        Assert.Equal((int)status, problem.GetProperty("status").GetInt32());
        Assert.Equal(code, problem.GetProperty("code").GetString());
        return problem;
    }
}
