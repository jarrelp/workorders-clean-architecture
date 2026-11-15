namespace WorkOrders.Application.Abstractions;

/// <summary>Saves all changes of one use case in a single transaction.</summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
