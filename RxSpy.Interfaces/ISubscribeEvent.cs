namespace RxSpy.Events;

public interface ISubscribeEvent : IEvent
{
    long ChildId { get; }
    long ParentId { get; }
}