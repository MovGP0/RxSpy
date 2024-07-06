using RxSpy.Events;

namespace RxSpy.Streaming;

public sealed partial class RxSpyStreamWriter : IRxSpyEventHandler
{
    public void OnCreated(OperatorCreatedEvent onCreatedEvent)
    {
        EnqueueEvent(onCreatedEvent);
    }

    public void OnCompleted(OnCompletedEvent onCompletedEvent)
    {
        EnqueueEvent(onCompletedEvent);
    }

    public void OnError(OnErrorEvent onErrorEvent)
    {
        EnqueueEvent(onErrorEvent);
    }

    public void OnNext(OnNextEvent onNextEvent)
    {
        EnqueueEvent(onNextEvent);
    }

    public void OnSubscribe(SubscribeEvent subscribeEvent)
    {
        EnqueueEvent(subscribeEvent);
    }

    public void OnUnsubscribe(UnsubscribeEvent unsubscribeEvent)
    {
        EnqueueEvent(unsubscribeEvent);
    }

    public void OnConnected(ConnectedEvent connectedEvent)
    {
        EnqueueEvent(connectedEvent);
    }

    public void OnDisconnected(DisconnectedEvent disconnectedEvent)
    {
        EnqueueEvent(disconnectedEvent);
    }

    public void OnTag(TagOperatorEvent tagEvent)
    {
        EnqueueEvent(tagEvent);
    }
}