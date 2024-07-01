namespace RxSpy.Events;

public interface IOnNextEvent : IEvent
{
    long OperatorId { get; }
    string ValueType { get; }
    string Value { get; }
    int Thread { get; }
}