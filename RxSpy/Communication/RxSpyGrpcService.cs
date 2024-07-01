using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RxSpy.Protobuf.Events;

namespace RxSpy.Communication;

public sealed class RxSpyGrpcService : RxSpyService.RxSpyServiceBase, IRxSpyEventHandler
{
    private readonly ConcurrentQueue<IMessage> _queue = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public override async Task GetEvents(
        Empty _,
        IServerStreamWriter<RxSpyEvents> responseStream,
        ServerCallContext context)
    {
        var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(
            _cancellationTokenSource.Token,
            context.CancellationToken)
            .Token;

        while (!cancellationToken.IsCancellationRequested)
        {
            while (_queue.TryDequeue(out var message)
                  && !cancellationToken.IsCancellationRequested)
            {
                var rxSpyEvent = ToRxSpyEvents(message);
                if (rxSpyEvent is null)
                {
                    continue;
                }

                await responseStream.WriteAsync(rxSpyEvent, cancellationToken);
            }

            await Task.Delay(10, cancellationToken);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnqueueEvent(IMessage ev) => _queue.Enqueue(ev);

    private static RxSpyEvents? ToRxSpyEvents(IMessage message)
    {
        return message switch
        {
            ConnectedEvent connectedEvent => new RxSpyEvents { Connected = connectedEvent },
            DisconnectedEvent disconnectedEvent => new RxSpyEvents { Disconnected = disconnectedEvent },
            SubscribeEvent subscribeEvent => new RxSpyEvents { Subscribe = subscribeEvent },
            UnsubscribeEvent unsubscribeEvent => new RxSpyEvents { Unsubscribe = unsubscribeEvent },
            OnNextEvent onNextEvent => new RxSpyEvents { OnNext = onNextEvent },
            OnErrorEvent onErrorEvent => new RxSpyEvents { OnError = onErrorEvent },
            OnCompletedEvent onCompletedEvent => new RxSpyEvents { OnCompleted = onCompletedEvent },
            OperatorCreatedEvent operatorCreatedEvent => new RxSpyEvents { OperatorCreated = operatorCreatedEvent },
            TagOperatorEvent tagOperatorEvent => new RxSpyEvents { TagOperator = tagOperatorEvent },
            _ => null
        };
    }

    #region IDisposable

    private bool _isDisposed;
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool _)
    {
        if (_isDisposed)
        {
            return;
        }

        _cancellationTokenSource.Cancel();
        _isDisposed = true;
    }

    ~RxSpyGrpcService() => Dispose(false);

    #endregion

    #region IRxSpyEventHandler

    public void OnCreated(OperatorCreatedEvent onCreatedEvent) => EnqueueEvent(onCreatedEvent);
    public void OnCompleted(OnCompletedEvent onCompletedEvent) => EnqueueEvent(onCompletedEvent);
    public void OnError(OnErrorEvent onErrorEvent) => EnqueueEvent(onErrorEvent);
    public void OnNext(OnNextEvent onNextEvent) => EnqueueEvent(onNextEvent);
    public void OnSubscribe(SubscribeEvent subscribeEvent) => EnqueueEvent(subscribeEvent);
    public void OnUnsubscribe(UnsubscribeEvent unsubscribeEvent) => EnqueueEvent(unsubscribeEvent);
    public void OnConnected(ConnectedEvent connectedEvent) => EnqueueEvent(connectedEvent);
    public void OnDisconnected(DisconnectedEvent disconnectedEvent) => EnqueueEvent(disconnectedEvent);
    public void OnTag(TagOperatorEvent tagEvent) => EnqueueEvent(tagEvent);

    #endregion
}