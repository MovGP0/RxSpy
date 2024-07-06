using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace RxSpy.AspNet;

public static class RxSpyHttpMiddlewareExtensions
{
    public static IServiceCollection AddRxSpyHttpMiddleware(IServiceCollection services)
    {
        services.AddSingleton<RxSpyHttpMiddleware>();
        return services;
    }

    public static void UseRxSpyHttpMiddleware(WebApplication app)
    {
        var middleware = app.Services.GetRequiredService<RxSpyHttpMiddleware>();
        app.Use(async (context, next) =>
        {
            await middleware.InvokeAsync(context, next);
        });
    }
}