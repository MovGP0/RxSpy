using System.Collections.Concurrent;
using ReactiveUI;
using DynamicData.Binding;
using Google.Protobuf;
using ReactiveUI.Fody.Helpers;
using RxSpy.Protobuf.Events;

namespace RxSpy.Models;

public class RxSpySessionModel : ReactiveObject
{
    readonly ConcurrentDictionary<long, RxSpyObservableModel> _observableRepository = new();

    readonly ConcurrentDictionary<long, RxSpySubscriptionModel> _subscriptionRepository = new();

    public ObservableCollectionExtended<RxSpyObservableModel> TrackedObservables { get; set; }

    [Reactive]
    public long SignalCount { get; set; }

    [Reactive]
    public long ErrorCount { get; set; }

    public RxSpySessionModel()
    {
        TrackedObservables = new ObservableCollectionExtended<RxSpyObservableModel>();
    }

    internal void OnEvent(IMessage ev)
    {
        switch (ev)
        {
            case OperatorCreatedEvent operatorCreatedEvent:
                OnOperatorCreated(operatorCreatedEvent);
                return;
            case SubscribeEvent subscribeEvent:
                OnSubscribe(subscribeEvent);
                return;
            case UnsubscribeEvent unsubscribeEvent:
                OnUnsubscribe(unsubscribeEvent);
                return;
            case OnCompletedEvent onCompletedEvent:
                OnCompleted(onCompletedEvent);
                return;
            case OnNextEvent onNextEvent:
                OnNext(onNextEvent);
                return;
            case OnErrorEvent onErrorEvent:
                OnError(onErrorEvent);
                return;
            case TagOperatorEvent tagOperatorEvent:
                OnTag(tagOperatorEvent);
                return;
            default:
                return;
        }
    }

    private void OnOperatorCreated(OperatorCreatedEvent operatorCreatedEvent)
    {
        var operatorModel = new RxSpyObservableModel(operatorCreatedEvent);

        _observableRepository.TryAdd(operatorCreatedEvent.Id, operatorModel);
        TrackedObservables.Add(operatorModel);
    }

    private void OnSubscribe(SubscribeEvent subscribeEvent)
    {
        _observableRepository.TryGetValue(subscribeEvent.ChildId, out var child);
        _observableRepository.TryGetValue(subscribeEvent.ParentId, out var parent);

        var subscriptionModel = new RxSpySubscriptionModel(subscribeEvent, child, parent)
        {
            IsActive = true
        };

        _subscriptionRepository.TryAdd(subscribeEvent.EventId, subscriptionModel);

        parent.Subscriptions.Add(subscriptionModel);

        parent.Children.Add(child);
        child.Parents.Add(parent);
    }

    private void OnUnsubscribe(UnsubscribeEvent unsubscribeEvent)
    {
        _subscriptionRepository.TryGetValue(unsubscribeEvent.SubscriptionId, out var subscriptionModel);

        if (subscriptionModel != null)
        {
            subscriptionModel.IsActive = false;
        }
    }

    private void OnError(OnErrorEvent onErrorEvent)
    {
        ErrorCount++;

        _observableRepository.TryGetValue(onErrorEvent.OperatorId, out var operatorModel);
        operatorModel?.OnError(onErrorEvent);
    }

    private void OnNext(OnNextEvent onNextEvent)
    {
        SignalCount++;

        _observableRepository.TryGetValue(onNextEvent.OperatorId, out var operatorModel);
        operatorModel?.OnNext(onNextEvent);
    }

    private void OnCompleted(OnCompletedEvent onCompletedEvent)
    {
        _observableRepository.TryGetValue(onCompletedEvent.OperatorId, out var operatorModel);
        operatorModel?.OnCompleted(onCompletedEvent);
    }

    private void OnTag(TagOperatorEvent tagOperatorEvent)
    {
        _observableRepository.TryGetValue(tagOperatorEvent.OperatorId, out var operatorModel);
        operatorModel?.OnTag(tagOperatorEvent);
    }
}