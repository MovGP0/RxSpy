using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace RxSpy.AspNet;

public sealed partial class RxSpyHttpMiddleware
{
    private readonly RxSpyOptions _options;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private static readonly ConcurrentDictionary<string, Channel<string>> Subscribers = new();

    public RxSpyHttpMiddleware(IOptions<RxSpyOptions> options)
    {
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Path.StartsWithSegments(_options.Endpoint))
        {
            await next(context);
            return;
        }

        if (_isDisposed)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return;
        }

        var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(
                _cancellationTokenSource.Token,
                context.RequestAborted)
            .Token;

        var subscriberId = context.TraceIdentifier;
        var channel = Channel.CreateUnbounded<string>();
        Subscribers.TryAdd(subscriberId, channel);

        context.Response.ContentType = "application/json";
        context.Response.Headers.TryAdd("Cache-Control", "no-cache");

        await using var responseStream = new StreamWriter(context.Response.Body, Encoding.UTF8);

        await foreach (var payload in channel.Reader.ReadAllAsync(cancellationToken))
        {
            await responseStream.WriteLineAsync(payload);
            await responseStream.FlushAsync(cancellationToken);
            await Task.Delay(10, cancellationToken);
        }

        Subscribers.TryRemove(subscriberId, out _);
    }

    internal static void EnqueueEvent(string jsonPayload)
    {
        if (Subscribers.IsEmpty)
        {
            return;
        }

        foreach (var subscriber in Subscribers.Values)
        {
            subscriber.Writer.TryWrite(jsonPayload);
        }
    }
}