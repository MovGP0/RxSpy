using RxSpy.Observables;

namespace System.Reactive.Linq
{
    public static class SpyObservableExtensions
    {
        public static IObservable<T> SpyTag<T>(this IObservable<T> source, string tag)
        {
            if (source is OperatorObservable<T> operatorObservable)
            {
                operatorObservable.Tag(tag);
            }

            return source;
        }
    }
}
