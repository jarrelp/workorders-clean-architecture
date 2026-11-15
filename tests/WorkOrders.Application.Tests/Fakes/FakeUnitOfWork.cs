using WorkOrders.Application.Abstractions;

namespace WorkOrders.Application.Tests.Fakes;

/// <summary>Counts saves, so a test can check that a failed use case stores nothing.</summary>
internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
