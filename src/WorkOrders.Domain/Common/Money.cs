using System.Globalization;

namespace WorkOrders.Domain.Common;

/// <summary>An amount in euros. Always rounded to whole cents and never negative.</summary>
public readonly record struct Money
{
    public const string Currency = "EUR";

    private Money(decimal amount) => Amount = RoundToCents(amount);

    public static Money Zero { get; } = new(0m);

    /// <summary>The amount in euros, with exactly two decimals.</summary>
    public decimal Amount { get; }

    public static Result<Money> FromEuros(decimal amount) =>
        amount < 0 ? MoneyErrors.Negative : new Money(amount);

    public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);

    /// <summary>Multiplies by a quantity or a number of hours and rounds the result to whole cents.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The factor is negative.</exception>
    public Money MultiplyBy(decimal factor)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(factor);
        return new Money(Amount * factor);
    }

    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Amount:0.00} {Currency}");

    // Commercial rounding: half a cent rounds away from zero. Adding 0.00m gives the result
    // exactly two decimals, so 12 is stored and serialized as 12.00.
    private static decimal RoundToCents(decimal amount) =>
        decimal.Round(amount, 2, MidpointRounding.AwayFromZero) + 0.00m;
}
