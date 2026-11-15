using WorkOrders.Application.Abstractions;

namespace WorkOrders.Infrastructure.Time;

/// <summary>Today's date in the time zone of the server or container (the TZ environment variable on Linux).</summary>
internal sealed class SystemClock(TimeProvider timeProvider) : IClock
{
    public DateOnly Today => DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
}
