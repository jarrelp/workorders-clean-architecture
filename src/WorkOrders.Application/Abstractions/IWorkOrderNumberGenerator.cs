using WorkOrders.Domain;

namespace WorkOrders.Application.Abstractions;

/// <summary>Hands out work order numbers: a new sequence per year.</summary>
public interface IWorkOrderNumberGenerator
{
    Task<WorkOrderNumber> NextAsync(int year, CancellationToken cancellationToken);
}
