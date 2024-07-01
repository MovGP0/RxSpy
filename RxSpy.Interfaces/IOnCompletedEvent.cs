namespace RxSpy.Events;

public interface IOnCompletedEvent : IEvent
{
    long OperatorId { get; }
}