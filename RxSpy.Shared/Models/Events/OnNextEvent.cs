using RxSpy.Events;

namespace RxSpy.Models.Events;

public sealed class OnNextEvent: Event, IOnNextEvent
{
    public long OperatorId { get; set; }
    public string ValueType { get; set; }
    public string Value { get; set; }
    public int Thread { get; set; }
}