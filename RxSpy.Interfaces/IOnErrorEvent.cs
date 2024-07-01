namespace RxSpy.Events;

public interface IOnErrorEvent : IEvent
{
    ITypeInfo ErrorType { get; }
    string Message { get; }
    long OperatorId { get; }
    string? StackTrace { get; }
}