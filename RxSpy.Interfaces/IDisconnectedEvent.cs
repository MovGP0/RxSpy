namespace RxSpy.Events;

public interface IDisconnectedEvent : IEvent
{
    long ConnectionId { get; }
}