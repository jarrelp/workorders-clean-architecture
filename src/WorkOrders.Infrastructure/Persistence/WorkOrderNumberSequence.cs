namespace WorkOrders.Infrastructure.Persistence;

/// <summary>The last work order number handed out in a year. One row per year.</summary>
internal sealed class WorkOrderNumberSequence(int year)
{
    public int Year { get; private set; } = year;

    /// <summary>A concurrency token: when two requests take the same number, the second save fails.</summary>
    public int LastValue { get; private set; }

    public int Next() => ++LastValue;
}
