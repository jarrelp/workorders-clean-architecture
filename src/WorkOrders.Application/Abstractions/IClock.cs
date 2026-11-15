namespace WorkOrders.Application.Abstractions;

/// <summary>The current date for business rules. Use this instead of reading the system clock.</summary>
public interface IClock
{
    DateOnly Today { get; }
}
