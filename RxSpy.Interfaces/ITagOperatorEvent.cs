namespace RxSpy.Events;

public interface ITagOperatorEvent : IEvent
{
    string Tag { get; }
    long OperatorId { get; }
}