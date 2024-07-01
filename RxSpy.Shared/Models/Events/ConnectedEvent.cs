using RxSpy.Events;

namespace RxSpy.Models.Events;

public sealed class ConnectedEvent: Event, IConnectedEvent
{
    public long OperatorId { get; set; }
}