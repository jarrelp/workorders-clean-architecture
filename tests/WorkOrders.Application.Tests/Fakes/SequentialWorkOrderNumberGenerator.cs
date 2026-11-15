using WorkOrders.Application.Abstractions;
using WorkOrders.Domain;

namespace WorkOrders.Application.Tests.Fakes;

/// <summary>Numbers 1, 2, 3 … per year, like the real generator, but in memory.</summary>
internal sealed class SequentialWorkOrderNumberGenerator : IWorkOrderNumberGenerator
{
    private readonly Dictionary<int, int> _lastSequencePerYear = [];

    public Task<WorkOrderNumber> NextAsync(int year, CancellationToken cancellationToken)
    {
        var next = _lastSequencePerYear.GetValueOrDefault(year) + 1;
        _lastSequencePerYear[year] = next;
        return Task.FromResult(WorkOrderNumber.Create(year, next));
    }
}
