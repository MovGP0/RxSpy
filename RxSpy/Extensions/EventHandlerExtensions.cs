using RxSpy.Entities;
using RxSpy.Events;
using RxSpy.Factories;

namespace RxSpy.Extensions;

public static class EventHandlerExtensions
{
    private static long Publish<T>(Action<T> publishAction, T ev) where T: IRxSpyEvent
    {
        publishAction(ev);
        return ev.EventId;
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