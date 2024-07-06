using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RxSpy.AspNet;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore;

public static class RxSpyHttpMiddlewareExtensions
{
    public static void UseRxSpy(this IApplicationBuilder app)
    {
        var middleware = app.ApplicationServices.GetRequiredService<RxSpyHttpMiddleware>();
        app.Use(async (context, next) =>
        {
            await middleware.InvokeAsync(context, next);
        });
    }
}