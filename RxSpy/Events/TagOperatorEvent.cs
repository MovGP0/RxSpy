namespace RxSpy.Events;

public struct TagOperatorEvent : IRxSpyEvent
{
    public long EventId { get; init; }
    public long EventTime { get; init; }
    public long OperatorId { get; init; }
    public string Tag { get; init; }
}