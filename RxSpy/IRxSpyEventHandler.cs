using RxSpy.Protobuf.Events;

namespace RxSpy;

public interface IRxSpyEventHandler: IDisposable
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