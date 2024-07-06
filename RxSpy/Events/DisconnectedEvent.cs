namespace RxSpy.Events;

public struct DisconnectedEvent : IRxSpyEvent
{
    public long EventId { get; init; }
    public long EventTime { get; init; }
    public long ConnectionId { get; init; }
}