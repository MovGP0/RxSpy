using RxSpy.Events;

namespace RxSpy.Models.Events;

public sealed class SubscribeEvent : Event, ISubscribeEvent
{
    public long ChildId { get; set; }
    public long ParentId { get; set; }
}