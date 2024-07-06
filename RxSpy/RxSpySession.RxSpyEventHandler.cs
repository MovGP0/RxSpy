using RxSpy.Events;

namespace RxSpy;

public sealed partial class RxSpySession: IRxSpyEventHandler
{
    public void OnCreated(OperatorCreatedEvent onCreatedEvent) => _eventHandler.OnCreated(onCreatedEvent);

    public void OnCompleted(OnCompletedEvent onCompletedEvent) => _eventHandler.OnCompleted(onCompletedEvent);

    public void OnError(OnErrorEvent onErrorEvent) => _eventHandler.OnError(onErrorEvent);

    public void OnNext(OnNextEvent onNextEvent) => _eventHandler.OnNext(onNextEvent);

    public void OnSubscribe(SubscribeEvent subscribeEvent) => _eventHandler.OnSubscribe(subscribeEvent);

    public void OnUnsubscribe(UnsubscribeEvent unsubscribeEvent) => _eventHandler.OnUnsubscribe(unsubscribeEvent);

    public void OnConnected(ConnectedEvent connectedEvent) => _eventHandler.OnConnected(connectedEvent);

    public void OnDisconnected(DisconnectedEvent disconnectedEvent) => _eventHandler.OnDisconnected(disconnectedEvent);

    public void OnTag(TagOperatorEvent tagEvent) => _eventHandler.OnTag(tagEvent);
}