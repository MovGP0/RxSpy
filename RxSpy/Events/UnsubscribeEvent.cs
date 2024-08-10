namespace RxSpy.Events;

public partial struct UnsubscribeEvent : IRxSpyEvent
{
    public long EventId { get; init; }
    public long EventTime { get; init; }
    public long SubscriptionId { get; init; }
}