using RxSpy.Events;

namespace RxSpy.Models.Events;

public sealed class UnsubscribeEvent: Event, IUnsubscribeEvent
{
    public long SubscriptionId { get; set; }
}