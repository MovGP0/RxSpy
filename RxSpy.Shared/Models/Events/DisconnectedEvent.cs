using RxSpy.Events;

namespace RxSpy.Models.Events;

public sealed class DisconnectedEvent : Event, IDisconnectedEvent
{
    public long ConnectionId { get; set; }
}