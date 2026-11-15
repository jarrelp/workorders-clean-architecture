using System.Globalization;
using System.Text.RegularExpressions;

namespace WorkOrders.Domain;

/// <summary>The number customers and technicians use for a work order, such as <c>WO-2026-0001</c>.</summary>
public sealed partial record WorkOrderNumber
{
    private WorkOrderNumber(string value) => Value = value;

    public string Value { get; }

    /// <summary>Creates the number for a sequence within a year.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The year has no four digits, or the sequence is not positive.</exception>
    public static WorkOrderNumber Create(int year, int sequence)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(year, 1000);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(year, 9999);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sequence);

        return new WorkOrderNumber(string.Create(CultureInfo.InvariantCulture, $"WO-{year}-{sequence:D4}"));
    }

    /// <summary>Restores a number that was created earlier, for example one read from the database.</summary>
    /// <exception cref="FormatException">The value is not a work order number.</exception>
    public static WorkOrderNumber Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return NumberFormat().IsMatch(value)
            ? new WorkOrderNumber(value)
            : throw new FormatException($"'{value}' is not a valid work order number.");
    }

    public override string ToString() => Value;

    [GeneratedRegex("^WO-[0-9]{4}-[0-9]{4,}$")]
    private static partial Regex NumberFormat();
}
