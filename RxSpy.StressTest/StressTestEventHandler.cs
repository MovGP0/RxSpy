using RxSpy.Protobuf.Events;

namespace RxSpy.StressTest;

public sealed class StressTestEventHandler: IRxSpyEventHandler
{
    private readonly IRxSpyEventHandler _inner;
    private int _eventCount;
    private int _observableCount;

    public int EventCount => _eventCount;
    public int ObservableCount => _observableCount;

    public StressTestEventHandler(IRxSpyEventHandler inner)
    {
        _inner = inner;
    }

    private void Increment()
    {
        Interlocked.Increment(ref _eventCount);
    }

    public void OnCreated(OperatorCreatedEvent onCreatedEvent)
    {
        Interlocked.Increment(ref _observableCount);
        Increment();
        _inner.OnCreated(onCreatedEvent);
    }

    public void OnCompleted(OnCompletedEvent onCompletedEvent)
    {
        Increment();
        _inner.OnCompleted(onCompletedEvent);
    }

    public void OnError(OnErrorEvent onErrorEvent)
    {
        Increment();
        _inner.OnError(onErrorEvent);
    }

    public void OnNext(OnNextEvent onNextEvent)
    {
        Increment();
        _inner.OnNext(onNextEvent);
    }

    public void OnSubscribe(SubscribeEvent subscribeEvent)
    {
        Increment();
        _inner.OnSubscribe(subscribeEvent);
    }

    public void OnUnsubscribe(UnsubscribeEvent unsubscribeEvent)
    {
        Increment();
        _inner.OnUnsubscribe(unsubscribeEvent);
    }

    public void OnConnected(ConnectedEvent connectedEvent)
    {
        Increment();
        _inner.OnConnected(connectedEvent);
    }

    public void OnDisconnected(DisconnectedEvent disconnectedEvent)
    {
        Increment();
        _inner.OnDisconnected(disconnectedEvent);
    }

    public void OnTag(TagOperatorEvent tagEvent)
    {
        Increment();
        _inner.OnTag(tagEvent);
    }

    public void Dispose()
    {
        _inner.Dispose();
    }
}