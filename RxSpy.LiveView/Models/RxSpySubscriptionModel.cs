using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using RxSpy.Protobuf.Events;

namespace RxSpy.Models;

public class RxSpySubscriptionModel: ReactiveObject
{
    [Reactive]
    public long SubscriptionId { get; set; }

    [Reactive]
    public RxSpyObservableModel Parent { get; set; }

    [Reactive]
    public RxSpyObservableModel Child { get; set; }

    [Reactive]
    public bool IsActive { get; set; }

    [Reactive]
    public TimeSpan Created { get; set; }

    public RxSpySubscriptionModel(
        SubscribeEvent subscribeEvent,
        RxSpyObservableModel child,
        RxSpyObservableModel parent)
    {
        SubscriptionId = subscribeEvent.BaseEvent.EventId;
        Parent = parent;
        Child = child;
        IsActive = true;
        Created = subscribeEvent.BaseEvent.EventTime.ToTimeSpan();
    }
}