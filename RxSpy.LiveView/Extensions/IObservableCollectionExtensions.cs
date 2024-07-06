using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;

namespace RxSpy.Extensions;

public static class ObservableCollectionExtensions
{
    public static IObservableCollection<TResult> CreateDerivedCollection<TItem, TResult>(
        this ObservableCollection<TItem> collection,
        Func<TItem, TResult> selector,
        Func<TItem, bool>? filter = null,
        // Func<TResult, TResult, int>? orderer = null,
        IScheduler? scheduler = null) where TItem : notnull
        where TResult : notnull
    {
        var result = new ObservableCollectionExtended<TResult>();

        var changeSet = collection.ToObservableChangeSet();

        var filtered = filter != null
            ? changeSet.Filter(filter)
            : changeSet;

        var transformed = filtered.Transform(selector);

        var scheduled = scheduler != null
            ? transformed.ObserveOn(scheduler)
            : transformed;

        scheduled.Bind(result);

        return result;
    }
}