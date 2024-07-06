using RxSpy;
using RxSpy.AspNet;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRxSpy(this IServiceCollection services)
    {
        services.AddSingleton<IRxSpyEventHandler, RxSpyHttpEventHandler>();
        services.AddSingleton<RxSpyHttpMiddleware>();
        return services;
    }
}