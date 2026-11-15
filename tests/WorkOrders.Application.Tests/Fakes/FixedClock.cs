using WorkOrders.Application.Abstractions;

namespace WorkOrders.Application.Tests.Fakes;

internal sealed class FixedClock(DateOnly today) : IClock
{
    public DateOnly Today { get; set; } = today;
}
