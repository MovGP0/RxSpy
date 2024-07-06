using System.Collections.Concurrent;
using System.Threading.Channels;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RxSpy.Protobuf.Events;

namespace RxSpy.Grpc;

public sealed partial class RxSpyGrpcService : RxSpyService.RxSpyServiceBase
{
    private static readonly ConcurrentDictionary<string, Channel<RxSpyEvents>> Subscribers = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public Task GetEvents(
        IServerStreamWriter<RxSpyEvents> responseStream,
        ServerCallContext context) =>
        GetEvents(new Empty(), responseStream, context);

    public override async Task GetEvents(
        Empty empty,
        IServerStreamWriter<RxSpyEvents> responseStream,
        ServerCallContext context)
    {
        if (_isDisposed)
        {
            context.Status = new Status(StatusCode.Unavailable, "RxSpy is not available");
            return;
        }

        var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(
                _cancellationTokenSource.Token,
                context.CancellationToken)
            .Token;

        var subscriberId = context.Host;
        var channel = Channel.CreateUnbounded<RxSpyEvents>();
        Subscribers.TryAdd(subscriberId, channel);

        await foreach (var payload in channel.Reader.ReadAllAsync(cancellationToken))
        {
            await responseStream.WriteAsync(payload, cancellationToken);
            await Task.Delay(10, cancellationToken);
        }

        Subscribers.TryRemove(subscriberId, out _);
    }

    internal static void Enqueue(RxSpyEvents rxSpyEvent)
    {
        if (Subscribers.IsEmpty)
        {
            return;
        }

        foreach (var subscriber in Subscribers.Values)
        {
            subscriber.Writer.TryWrite(rxSpyEvent);
        }
    }
}