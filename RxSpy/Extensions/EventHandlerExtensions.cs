using Google.Protobuf;
using RxSpy.Protobuf.Events;

namespace RxSpy.Events;

public static class EventHandlerExtensions
{
    private static long Publish<T>(Action<T> publishAction, T ev) where T: IMessage
    {
        publishAction(ev);
        return GetEventId(ev);
    }

    private static long GetEventId(IMessage message)
    {
        return message switch
        {
            ConnectedEvent connectedEvent => connectedEvent.BaseEvent.EventId,
            DisconnectedEvent disconnectedEvent => disconnectedEvent.BaseEvent.EventId,
            TagOperatorEvent tagEvent => tagEvent.BaseEvent.EventId,
            UnsubscribeEvent unsubscribeEvent => unsubscribeEvent.BaseEvent.EventId,
            SubscribeEvent subscribeEvent => subscribeEvent.BaseEvent.EventId,
            OnCompletedEvent completedEvent => completedEvent.BaseEvent.EventId,
            OnErrorEvent errorEvent => errorEvent.BaseEvent.EventId,
            OnNextEvent nextEvent => nextEvent.BaseEvent.EventId,
            OperatorCreatedEvent createdEvent => createdEvent.BaseEvent.EventId,
            _ => -1L
        };
    }

    public static long OnConnected(this IRxSpyEventHandler eventHandler, OperatorInfo operatorInfo)
    {
        return Publish(eventHandler.OnConnected, EventFactory.Connect(operatorInfo));
    }

    public static long OnDisconnected(this IRxSpyEventHandler eventHandler, long subscriptionId)
    {
        return Publish(eventHandler.OnDisconnected, EventFactory.Disconnect(subscriptionId));
    }

    public static long OnSubscribe(this IRxSpyEventHandler eventHandler, OperatorInfo child, OperatorInfo parent)
    {
        return Publish(eventHandler.OnSubscribe, EventFactory.Subscribe(child, parent));
    }

    public static long OnUnsubscribe(this IRxSpyEventHandler eventHandler, long subscriptionId)
    {
        return Publish(eventHandler.OnUnsubscribe, EventFactory.Unsubscribe(subscriptionId));
    }
}