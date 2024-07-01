namespace RxSpy.Events;

public enum EventType
{
    OperatorCreated,
    OperatorCollected,
    Subscribe,
    Unsubscribe,

    OnNext,
    OnError,
    OnCompleted,

    TagOperator,

    Connected,
    Disconnected
}