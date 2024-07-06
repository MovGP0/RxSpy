namespace RxSpy.Constants;

internal static class ObserverMethodNames
{
    /// <seealso cref="IObserver{T}.OnNext"/>
    public const string OnNext = "OnNext";

    /// <seealso cref="IObserver{T}.OnCompleted"/>
    internal const string OnCompleted = "OnCompleted";

    /// <seealso cref="IObserver{T}.OnError"/>
    internal const string OnError = "OnError";
}