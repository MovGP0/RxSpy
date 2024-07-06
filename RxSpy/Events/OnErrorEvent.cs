using RxSpy.Entities;

namespace RxSpy.Events;

public struct OnErrorEvent : IRxSpyEvent
{
    public long EventId { get; init; }
    public long EventTime { get; init; }
    public TypeInfo ErrorType { get; init; }
    public string Message { get; init; }
    public long OperatorId { get; init; }
    public string StackTrace { get; init; }
}