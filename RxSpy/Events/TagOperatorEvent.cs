namespace RxSpy.Events;

public partial struct TagOperatorEvent : IRxSpyEvent
{
    public long EventId { get; init; }
    public long EventTime { get; init; }
    public long OperatorId { get; init; }
    public string Tag { get; init; }
}