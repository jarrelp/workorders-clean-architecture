using WorkOrders.Domain.Common;

namespace WorkOrders.Domain;

/// <summary>Time a technician spent on a work order on one day. Only created through <see cref="WorkOrder.LogTime"/>.</summary>
public sealed class TimeEntry
{
    private TimeEntry(string technician, DateOnly date, int minutes)
    {
        Technician = technician;
        Date = date;
        Minutes = minutes;
    }

    public string Technician { get; }

    public DateOnly Date { get; }

    public int Minutes { get; }

    internal static Result<TimeEntry> Create(string technician, DateOnly date, int minutes)
    {
        if (string.IsNullOrWhiteSpace(technician))
        {
            return WorkOrderErrors.TechnicianRequired;
        }

        if (minutes <= 0)
        {
            return WorkOrderErrors.MinutesMustBePositive;
        }

        return new TimeEntry(technician.Trim(), date, minutes);
    }
}
