using RxSpy.Events;

namespace RxSpy;

public interface IRxSpyEventHandler
{
    void OnCreated(OperatorCreatedEvent onCreatedEvent);
    void OnCompleted(OnCompletedEvent onCompletedEvent);
    void OnError(OnErrorEvent onErrorEvent);
    void OnNext(OnNextEvent onNextEvent);
    void OnSubscribe(SubscribeEvent subscribeEvent);
    void OnUnsubscribe(UnsubscribeEvent unsubscribeEvent);
    void OnConnected(ConnectedEvent connectedEvent);
    void OnDisconnected(DisconnectedEvent disconnectedEvent);
    void OnTag(TagOperatorEvent tagEvent);
}