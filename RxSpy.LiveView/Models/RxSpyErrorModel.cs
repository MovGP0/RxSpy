using RxSpy.Protobuf.Events;

namespace RxSpy.Models;

public class RxSpyErrorModel
{
    public TypeInfo ErrorType { get; set; }
    public string Message { get; set; }
    public TimeSpan Received { get; set; }
    public string StackTrace { get; set; }

    public RxSpyErrorModel(OnErrorEvent onErrorEvent)
    {
        Received = onErrorEvent.BaseEvent.EventTime.ToTimeSpan();
        ErrorType = onErrorEvent.ErrorType;
        Message = onErrorEvent.Message;
        StackTrace = onErrorEvent.StackTrace;
    }
}