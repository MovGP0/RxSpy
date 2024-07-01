namespace RxSpy.Events;

public interface IUnsubscribeEvent : IEvent
{
    long SubscriptionId { get; }
}