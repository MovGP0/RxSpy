using RxSpy.Entities;

namespace RxSpy.Events;

public partial struct OperatorCreatedEvent : IRxSpyEvent
{
    public long EventId { get; init; }
    public long EventTime { get; init; }
    public long Id { get; init; }
    public string Name { get; init; }
    public CallSite CallSite { get; init; }
    public MethodInfo OperatorMethod { get; init; }
}