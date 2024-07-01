namespace RxSpy.Events;

public interface IEvent
{
    EventType EventType { get; }
    long EventId { get; }
    long EventTime { get; }
}