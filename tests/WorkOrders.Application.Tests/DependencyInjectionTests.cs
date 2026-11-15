using Microsoft.Extensions.DependencyInjection;

namespace WorkOrders.Application.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_RegistersEveryHandlerAsScoped()
    {
        var services = new ServiceCollection().AddApplication();

        var handlerTypes = typeof(DependencyInjection).Assembly
            .GetExportedTypes()
            .Where(type => type.Name.EndsWith("Handler", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(handlerTypes);
        Assert.All(handlerTypes, handlerType =>
            Assert.Contains(services, service => service.ServiceType == handlerType && service.Lifetime == ServiceLifetime.Scoped));
    }
}
