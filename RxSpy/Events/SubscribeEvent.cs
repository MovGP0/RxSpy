namespace RxSpy.Events;

public partial struct SubscribeEvent : IRxSpyEvent
{
    public long EventId { get; init; }
    public long EventTime { get; init; }
    public long ChildId { get; init; }
    public long ParentId { get; init; }
}