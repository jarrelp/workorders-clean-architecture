namespace WorkOrders.Domain.Common;

public static class MoneyErrors
{
    public static readonly Error Negative = Error.Validation("Money.Negative", "An amount cannot be negative.");
}
