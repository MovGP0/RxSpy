using RxSpy.Events;

namespace RxSpy.Models.Events;

public sealed class TagOperatorEvent: Event, ITagOperatorEvent
{
    public long OperatorId { get; set; }
    public string Tag { get; set; }
}