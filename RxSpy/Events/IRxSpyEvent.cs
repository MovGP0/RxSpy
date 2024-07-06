namespace RxSpy.Events;

public interface IRxSpyEvent
{
    public long EventId { get; init; }
    public long EventTime { get; init; }
}