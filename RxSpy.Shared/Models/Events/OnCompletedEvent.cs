using RxSpy.Events;

namespace RxSpy.Models.Events;

public sealed class OnCompletedEvent: Event, IOnCompletedEvent
{
    public long OperatorId { get; set; }
}