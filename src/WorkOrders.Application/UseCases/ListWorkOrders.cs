using WorkOrders.Application.Abstractions;
using WorkOrders.Application.Validation;
using WorkOrders.Domain;
using WorkOrders.Domain.Common;

namespace WorkOrders.Application.UseCases;

/// <param name="Status">Optional status name, such as <c>Scheduled</c>. Case-insensitive.</param>
public sealed record ListWorkOrdersQuery(string? Status);

/// <summary>Lists work orders by number, optionally only those with one status.</summary>
public sealed class ListWorkOrdersHandler(IWorkOrderRepository workOrders)
{
    public async Task<Result<IReadOnlyList<WorkOrderSummaryResponse>>> HandleAsync(
        ListWorkOrdersQuery query,
        CancellationToken cancellationToken)
    {
        WorkOrderStatus? status = null;
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!TryParseStatus(query.Status, out var parsed))
            {
                return UnknownStatus();
            }

            status = parsed;
        }

        var found = await workOrders.ListAsync(status, cancellationToken);
        return Result.Success<IReadOnlyList<WorkOrderSummaryResponse>>([.. found.Select(workOrder => workOrder.ToSummary())]);
    }

    // Accepts the status names only, not the numbers behind the enum.
    private static bool TryParseStatus(string value, out WorkOrderStatus status)
    {
        foreach (var candidate in Enum.GetValues<WorkOrderStatus>())
        {
            if (string.Equals(candidate.ToString(), value.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                status = candidate;
                return true;
            }
        }

        status = default;
        return false;
    }

    private static ValidationError UnknownStatus() =>
        new(new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            [nameof(ListWorkOrdersQuery.Status)] = [$"Must be one of: {string.Join(", ", Enum.GetNames<WorkOrderStatus>())}."],
        });
}
