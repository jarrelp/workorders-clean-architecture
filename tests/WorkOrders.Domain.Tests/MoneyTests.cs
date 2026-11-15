using System.Globalization;
using WorkOrders.Domain.Common;
using static WorkOrders.Domain.Tests.TestWorkOrders;

namespace WorkOrders.Domain.Tests;

public class MoneyTests
{
    public static TheoryData<decimal, string> RoundingCases => new()
    {
        { 10.004m, "10.00" },
        { 10.005m, "10.01" },
        { 0.125m, "0.13" },
        { 12m, "12.00" },
    };

    public static TheoryData<decimal, decimal, decimal> MultiplicationCases => new()
    {
        { 24.95m, 3m, 74.85m },
        { 6.40m, 2.5m, 16.00m },
        { 65.00m, 50m / 60m, 54.17m },
    };

    [Theory]
    [MemberData(nameof(RoundingCases))]
    public void FromEuros_RoundsToWholeCents_HalfACentRoundsUp(decimal amount, string expected)
    {
        var money = Money.FromEuros(amount).Value;

        Assert.Equal(expected, money.Amount.ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    public void FromEuros_WithNegativeAmount_Fails()
    {
        var result = Money.FromEuros(-0.01m);

        ResultAssert.Failed(MoneyErrors.Negative, result);
    }

    [Fact]
    public void Addition_AddsTheAmounts()
    {
        Assert.Equal(Euros(10.30m), Euros(10.10m) + Euros(0.20m));
    }

    [Theory]
    [MemberData(nameof(MultiplicationCases))]
    public void MultiplyBy_RoundsTheResultToWholeCents(decimal amount, decimal factor, decimal expected)
    {
        Assert.Equal(expected, Euros(amount).MultiplyBy(factor).Amount);
    }

    [Fact]
    public void MultiplyBy_NegativeFactor_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Euros(10m).MultiplyBy(-1m));
    }
}
