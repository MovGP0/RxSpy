namespace RxSpy.Events;

public partial struct OnCompletedEvent : IRxSpyEvent
{
    public long EventId { get; init; }
    public long EventTime { get; init; }
    public long OperatorId { get; init; }
}