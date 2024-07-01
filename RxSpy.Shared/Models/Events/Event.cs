using RxSpy.Events;

namespace RxSpy.Models.Events;

public abstract class Event: IEvent
{
    public EventType EventType { get; set; }
    public long EventId { get; set; }
    public long EventTime { get; set; }
}