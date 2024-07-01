using RxSpy.Events;

namespace RxSpy.Models.Events;

public sealed class OnErrorEvent: Event, IOnErrorEvent
{
    public ITypeInfo ErrorType { get; set; }
    public string Message { get; set; }
    public string StackTrace { get; set; }
    public long OperatorId { get; set; }
}