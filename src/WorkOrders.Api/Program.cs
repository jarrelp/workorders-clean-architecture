using WorkOrders.Api.Endpoints;
using WorkOrders.Application;
using WorkOrders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Composition root: the only place that knows every layer. Each layer registers its own services.
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// Every error response is an RFC 9457 problem details document.
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context => context.ProblemDetails.Instance ??= context.HttpContext.Request.Path);
builder.Services.AddOpenApi();

// Reject a body with a missing field or a null for a non-nullable property, instead of using a default value.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.RespectNullableAnnotations = true;
    options.SerializerOptions.RespectRequiredConstructorParameters = true;
});

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    // Malformed JSON or a missing body is the client's mistake: 400, not 500.
    StatusCodeSelector = exception => exception is BadHttpRequestException badRequest
        ? badRequest.StatusCode
        : StatusCodes.Status500InternalServerError,
});
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");
app.MapWorkOrderEndpoints();

await app.RunAsync();
