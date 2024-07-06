using System.Reactive.Linq;

namespace RxSpy.Constants;

internal static class QueryLanguageMethodNames
{
    /// <seealso cref="IQueryLanguage.Create{TSource}(System.Func{System.IObserver{TSource},System.IDisposable})"/>
    internal const string Create = "Create";

    /// <seealso cref="IQueryLanguage.Subscribe{TSource}(System.Collections.Generic.IEnumerable{TSource},System.IObserver{TSource})"/>
    internal const string Subscribe = "Subscribe";

    /// <seealso cref="IQueryLanguage.Next{TSource}"/>
    internal const string OnNext = "Next";

    /// <seealso cref="IQueryLanguage.Multicast{TSource,TResult}"/>
    internal const string Multicast = "Multicast";
    
    /// <seealso cref="IQueryLanguage.Publish{TSource}(System.IObservable{TSource})"/>
    internal const string Publish = "Publish";

    /// <seealso cref="IQueryLanguage.PublishLast{TSource}"/>
    internal const string PublishLast = "PublishLast";

    /// <seealso cref="IQueryLanguage.Replay{TSource}(System.IObservable{TSource})"/>
    internal const string Replay = "Replay";
    
    /// <seealso cref="IQueryLanguage.Wait{TSource}"/>
    internal const string Wait = "Wait";

    /// <seealso cref="IQueryLanguage.First{TSource}(System.IObservable{TSource})"/>
    internal const string First = "First";

    /// <seealso cref="IQueryLanguage.FirstOrDefault{TSource}(System.IObservable{TSource})"/>
    internal const string FirstOrDefault = "FirstOrDefault";
    
    /// <seealso cref="IQueryLanguage.Last{TSource}(System.IObservable{TSource})"/>
    internal const string Last = "Last";
    
    /// <seealso cref="IQueryLanguage.LastOrDefault{TSource}(System.IObservable{TSource})"/>
    internal const string LastOrDefault = "LastOrDefault";

    /// <seealso cref="IQueryLanguage.Single{TSource}(System.IObservable{TSource})"/>
    internal const string ForEach = "ForEach";
}