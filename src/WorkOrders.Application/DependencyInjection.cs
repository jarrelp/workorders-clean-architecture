using Microsoft.Extensions.DependencyInjection;
using WorkOrders.Application.UseCases;

namespace WorkOrders.Application;

public static class DependencyInjection
{
    /// <summary>Registers one handler per use case. The list doubles as an index of what the application can do.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateWorkOrderHandler>();
        services.AddScoped<GetWorkOrderHandler>();
        services.AddScoped<ListWorkOrdersHandler>();
        services.AddScoped<ScheduleWorkOrderHandler>();
        services.AddScoped<StartWorkOrderHandler>();
        services.AddScoped<LogTimeHandler>();
        services.AddScoped<AddMaterialHandler>();
        services.AddScoped<CompleteWorkOrderHandler>();
        services.AddScoped<CancelWorkOrderHandler>();

        return services;
    }
}
