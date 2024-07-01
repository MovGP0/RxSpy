using RxSpy.Events;

namespace RxSpy.Models.Events;

public sealed class OperatorCreatedEvent : Event, IOperatorCreatedEvent
{
    public long Id { get; set; }
    public string Name { get; set; }
    public ICallSite CallSite { get; set; }
    public IMethodInfo OperatorMethod { get; set; }
}