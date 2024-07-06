using System.Reactive.Linq;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using RxSpy.Protobuf.Events;

namespace RxSpy.Models;

public sealed class RxSpyObservableModel: ReactiveObject
{
    public long Id { get; set; }
    public string Name { get; set; }

    public MethodInfo OperatorMethod { get; private set; }
    public CallSite CallSite { get; private set; }

    [Reactive]
    public string Tag { get; set; }

    public TimeSpan Created { get; private set; }

    [Reactive]
    public ObservableCollectionExtended<RxSpyObservableModel> Parents { get; set; }

    [Reactive]
    public ObservableCollectionExtended<RxSpyObservableModel> Children { get; set; }

    [Reactive]
    public ObservableCollectionExtended<RxSpySubscriptionModel> Subscriptions { get; set; }

    [Reactive]
    public ObservableCollectionExtended<RxSpyObservedValueModel> ObservedValues { get; set; }

    [Reactive]
    public RxSpyErrorModel Error { get; set; }

    readonly ObservableAsPropertyHelper<bool> _hasError;
    public bool HasError => _hasError.Value;

    [Reactive]
    public bool IsActive { get; set; }

    [Reactive]
    public long ValuesProduced { get; set; }

    readonly ObservableAsPropertyHelper<int> _descendants;
    public int Descendants => _descendants.Value;

    readonly ObservableAsPropertyHelper<int> _ancestors;
    public int Ancestors => _ancestors.Value;

    [Reactive]
    public string Status { get; set; }

    public RxSpyObservableModel(OperatorCreatedEvent createdEvent)
    {
        Id = createdEvent.Id;
        Name = createdEvent.Name;
        OperatorMethod = createdEvent.OperatorMethod;
        CallSite = createdEvent.CallSite;
        IsActive = true;
        Created = createdEvent.EventTime.ToTimeSpan();
        Subscriptions = new ObservableCollectionExtended<RxSpySubscriptionModel>();
        Parents = new ObservableCollectionExtended<RxSpyObservableModel>();
        Children = new ObservableCollectionExtended<RxSpyObservableModel>();
        ObservedValues = new ObservableCollectionExtended<RxSpyObservedValueModel>();

        this.WhenAnyValue(x => x.Error)
            .Select(x => x == null ? false : true)
            .ToProperty(this, x => x.HasError, out _hasError);

        this.WhenAnyValue(x => x.Children.Count)
            .Select(_ => Children.Select(c => c.WhenAnyValue(x => x.Descendants)).CombineLatest())
            .Switch()
            .Select(x => x.Sum() + Children.Count)
            .ToProperty(this, x => x.Descendants, out _descendants);

        this.WhenAnyValue(x => x.Parents.Count)
            .Select(_ => Parents.Select(c => c.WhenAnyValue(x => x.Ancestors)).CombineLatest())
            .Switch()
            .Select(x => x.Sum() + Parents.Count)
            .ToProperty(this, x => x.Ancestors, out _ancestors);

        Status = "Active";
    }

    public void OnNext(OnNextEvent onNextEvent)
    {
        ObservedValues.Add(new RxSpyObservedValueModel(onNextEvent));
        ValuesProduced++;
    }

    public void OnCompleted(OnCompletedEvent onCompletedEvent)
    {
        IsActive = false;
        Status = "Completed";
    }

    public void OnError(OnErrorEvent onErrorEvent)
    {
        Error = new RxSpyErrorModel(onErrorEvent);
        IsActive = false;
        Status = "Error";
    }

    public void OnTag(TagOperatorEvent onTagEvent)
    {
        Tag = onTagEvent.Tag;
    }
}