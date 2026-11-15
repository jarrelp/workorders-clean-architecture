using WorkOrders.Application.Abstractions;
using WorkOrders.Domain;

namespace WorkOrders.Infrastructure.Persistence;

/// <summary>
/// Takes the next number from a per-year counter table. The counter is saved in the same
/// transaction as the new work order, so a failed request does not use up a number.
/// </summary>
internal sealed class WorkOrderNumberGenerator(WorkOrdersDbContext dbContext) : IWorkOrderNumberGenerator
{
    public async Task<WorkOrderNumber> NextAsync(int year, CancellationToken cancellationToken)
    {
        var sequence = await dbContext.WorkOrderNumberSequences.FindAsync([year], cancellationToken);
        if (sequence is null)
        {
            sequence = new WorkOrderNumberSequence(year);
            dbContext.WorkOrderNumberSequences.Add(sequence);
        }

        return WorkOrderNumber.Create(year, sequence.Next());
    }
}
