using RxSpy.Observables;

// ReSharper disable once CheckNamespace
namespace System.Reactive.Linq;

public static class SpyObservableExtensions
{
    public static TSource SpyTag<TSource, T>(this TSource source, string tag)
        where TSource:IObservable<T>
    {
        if (source is OperatorObservable<T> operatorObservable)
        {
            operatorObservable.Tag(tag);
        }

        return source;
    }
}