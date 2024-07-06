using RxSpy.Events;

namespace RxSpy.Grpc;

public sealed class RxSpyGrpcEventHandler : IRxSpyEventHandler
{
    public void OnCreated(OperatorCreatedEvent onCreatedEvent) => RxSpyGrpcService.Enqueue(onCreatedEvent.ToRxSpyEvents());

    public void OnCompleted(OnCompletedEvent onCompletedEvent) => RxSpyGrpcService.Enqueue(onCompletedEvent.ToRxSpyEvents());

    public void OnError(OnErrorEvent onErrorEvent) => RxSpyGrpcService.Enqueue(onErrorEvent.ToRxSpyEvents());

    public void OnNext(OnNextEvent onNextEvent) => RxSpyGrpcService.Enqueue(onNextEvent.ToRxSpyEvents());

    public void OnSubscribe(SubscribeEvent subscribeEvent) => RxSpyGrpcService.Enqueue(subscribeEvent.ToRxSpyEvents());

    public void OnUnsubscribe(UnsubscribeEvent unsubscribeEvent) => RxSpyGrpcService.Enqueue(unsubscribeEvent.ToRxSpyEvents());

    public void OnConnected(ConnectedEvent connectedEvent) => RxSpyGrpcService.Enqueue(connectedEvent.ToRxSpyEvents());

    public void OnDisconnected(DisconnectedEvent disconnectedEvent) => RxSpyGrpcService.Enqueue(disconnectedEvent.ToRxSpyEvents());

    public void OnTag(TagOperatorEvent tagEvent) => RxSpyGrpcService.Enqueue(tagEvent.ToRxSpyEvents());
}