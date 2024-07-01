namespace RxSpy.Events;

public interface IOperatorCreatedEvent : IEvent
{
    long Id { get; }
    string Name { get; }
    ICallSite CallSite { get; }
    IMethodInfo OperatorMethod { get; }
}