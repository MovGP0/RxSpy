using RxSpy.Protobuf.Events;

namespace RxSpy.Models;

public class RxSpyObservedValueModel
{
    public string ValueType { get; set; }
    public string Value { get; set; }
    public TimeSpan Received { get; set; }
    public int Thread { get; set; }

    public RxSpyObservedValueModel(OnNextEvent onNextEvent)
    {
        ValueType = onNextEvent.ValueType;
        Value = onNextEvent.Value;
        Thread = onNextEvent.Thread;
        Received = onNextEvent.BaseEvent.EventTime.ToTimeSpan();
    }
}