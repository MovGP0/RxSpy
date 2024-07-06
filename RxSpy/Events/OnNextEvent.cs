namespace RxSpy.Events;

public struct OnNextEvent : IRxSpyEvent
{
    public long EventId { get; init; }
    public long EventTime { get; init; }
    public long OperatorId { get; init; }
    public string ValueType { get; init; }
    public string Value { get; init; }
    public int Thread { get; init; }
}