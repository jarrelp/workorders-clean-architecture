using Microsoft.AspNetCore.Http.HttpResults;
using WorkOrders.Application.UseCases;

namespace WorkOrders.Api.Endpoints;

/// <summary>
/// The HTTP surface of the work order feature. Each endpoint translates the request into a
/// command or query, calls one handler and translates the result. There is no logic here.
/// </summary>
internal static class WorkOrderEndpoints
{
    private const string GetWorkOrderRoute = "GetWorkOrder";

    public static IEndpointRouteBuilder MapWorkOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var workOrders = endpoints.MapGroup("/work-orders").WithTags("Work orders");

        workOrders.MapPost("/", CreateAsync)
            .WithSummary("Register a reported problem as a new work order (Draft).")
            .ProducesValidationProblem();

        workOrders.MapGet("/", ListAsync)
            .WithSummary("List work orders by number, optionally only those with one status.")
            .ProducesValidationProblem();

        workOrders.MapGet("/{id:guid}", GetAsync)
            .WithName(GetWorkOrderRoute)
            .WithSummary("Get a work order with its time entries, materials and totals.")
            .ProducesProblem(StatusCodes.Status404NotFound);

        workOrders.MapPost("/{id:guid}/schedule", ScheduleAsync)
            .WithSummary("Assign a technician and a visit date (Draft to Scheduled).")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        workOrders.MapPost("/{id:guid}/start", StartAsync)
            .WithSummary("Start the work on site (Scheduled to InProgress).")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        workOrders.MapPost("/{id:guid}/time-entries", LogTimeAsync)
            .WithSummary("Log time spent by a technician (InProgress only).")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        workOrders.MapPost("/{id:guid}/materials", AddMaterialAsync)
            .WithSummary("Add used material (InProgress only).")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        workOrders.MapPost("/{id:guid}/complete", CompleteAsync)
            .WithSummary("Finish the work (InProgress to Completed). Needs at least one time entry.")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        workOrders.MapPost("/{id:guid}/cancel", CancelAsync)
            .WithSummary("Call off a work order that is not completed yet.")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static async Task<Results<CreatedAtRoute<WorkOrderResponse>, ProblemHttpResult>> CreateAsync(
        CreateWorkOrderRequest request,
        CreateWorkOrderHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateWorkOrderCommand(
            request.CustomerName,
            request.SiteDescription,
            request.ProblemDescription,
            request.HourlyRate);

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, GetWorkOrderRoute, new { id = result.Value.Id })
            : result.Error.ToProblem();
    }

    private static async Task<Results<Ok<IReadOnlyList<WorkOrderSummaryResponse>>, ProblemHttpResult>> ListAsync(
        string? status,
        ListWorkOrdersHandler handler,
        CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new ListWorkOrdersQuery(status), cancellationToken)).ToOkOrProblem();

    private static async Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> GetAsync(
        Guid id,
        GetWorkOrderHandler handler,
        CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new GetWorkOrderQuery(id), cancellationToken)).ToOkOrProblem();

    private static async Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> ScheduleAsync(
        Guid id,
        ScheduleWorkOrderRequest request,
        ScheduleWorkOrderHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new ScheduleWorkOrderCommand(id, request.Technician, request.ScheduledDate);
        return (await handler.HandleAsync(command, cancellationToken)).ToOkOrProblem();
    }

    private static async Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> StartAsync(
        Guid id,
        StartWorkOrderHandler handler,
        CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new StartWorkOrderCommand(id), cancellationToken)).ToOkOrProblem();

    private static async Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> LogTimeAsync(
        Guid id,
        LogTimeRequest request,
        LogTimeHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new LogTimeCommand(id, request.Technician, request.Date, request.Minutes);
        return (await handler.HandleAsync(command, cancellationToken)).ToOkOrProblem();
    }

    private static async Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> AddMaterialAsync(
        Guid id,
        AddMaterialRequest request,
        AddMaterialHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new AddMaterialCommand(id, request.Description, request.Quantity, request.UnitPrice);
        return (await handler.HandleAsync(command, cancellationToken)).ToOkOrProblem();
    }

    private static async Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> CompleteAsync(
        Guid id,
        CompleteWorkOrderHandler handler,
        CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new CompleteWorkOrderCommand(id), cancellationToken)).ToOkOrProblem();

    private static async Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> CancelAsync(
        Guid id,
        CancelWorkOrderHandler handler,
        CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new CancelWorkOrderCommand(id), cancellationToken)).ToOkOrProblem();
}
