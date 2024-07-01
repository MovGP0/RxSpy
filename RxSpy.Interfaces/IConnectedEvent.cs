namespace RxSpy.Events;

public interface IConnectedEvent : IEvent
{
    long OperatorId { get; }
}