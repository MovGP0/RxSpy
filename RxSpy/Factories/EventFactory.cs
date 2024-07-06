using freakcode.frequency;
using RxSpy.Entities;
using RxSpy.Events;
using RxSpy.Utils;
using Type = System.Type;

namespace RxSpy.Factories;

internal abstract class EventFactory
{
    private static long _counter;

    public static OperatorCreatedEvent OperatorCreated(OperatorInfo operatorInfo)
    {
        return new()
        {
            EventId = Interlocked.Increment(ref _counter),
            EventTime = Monotonic.Time(),
            CallSite = operatorInfo.CallSite,
            Id = operatorInfo.Id,
            Name = operatorInfo.Name,
            OperatorMethod = operatorInfo.OperatorMethod
        };
    }

    public static OnNextEvent OnNext(OperatorInfo operatorInfo, Type valueType, object? value)
    {
        return new()
        {
            EventId = Interlocked.Increment(ref _counter),
            EventTime = Monotonic.Time(),
            OperatorId = operatorInfo.Id,
            Thread = Thread.CurrentThread.ManagedThreadId,
            Value = ValueFormatter.ToString(value, valueType),
            ValueType = TypeUtils.ToFriendlyName(valueType)
        };
    }

    public static OnErrorEvent OnError(OperatorInfo operatorInfo, Exception? error)
    {
        if (error is null)
        {
            return new()
            {
                EventId = Interlocked.Increment(ref _counter),
                EventTime = Monotonic.Time(),
                OperatorId = operatorInfo.Id
            };
        }

        return new()
        {
            EventId = Interlocked.Increment(ref _counter),
            EventTime = Monotonic.Time(),
            OperatorId = operatorInfo.Id,
            ErrorType = TypeInfoFactory.Create(error.GetType()),
            Message = error.Message,
            StackTrace = error.StackTrace
        };
    }

    public static OnCompletedEvent OnCompleted(OperatorInfo operatorInfo)
    {
        return new()
        {
            EventId = Interlocked.Increment(ref _counter),
            EventTime = Monotonic.Time(),
            OperatorId = operatorInfo.Id
        };
    }

    internal static SubscribeEvent Subscribe(OperatorInfo observer, OperatorInfo source)
    {
        return new()
        {
            EventId = Interlocked.Increment(ref _counter),
            EventTime = Monotonic.Time(),
            ChildId = observer.Id,
            ParentId = source.Id
        };
    }

    internal static UnsubscribeEvent Unsubscribe(long subscriptionId)
    {
        return new()
        {
            EventId = Interlocked.Increment(ref _counter),
            EventTime = Monotonic.Time(),
            SubscriptionId = subscriptionId
        };
    }

    internal static TagOperatorEvent Tag(OperatorInfo operatorInfo, string tag)
    {
        return new()
        {
            EventId = Interlocked.Increment(ref _counter),
            EventTime = Monotonic.Time(),
            OperatorId = operatorInfo.Id,
            Tag = tag
        };
    }

    internal static ConnectedEvent Connect(OperatorInfo operatorInfo)
    {
        return new()
        {
            EventId = Interlocked.Increment(ref _counter),
            EventTime = Monotonic.Time(),
            OperatorId = operatorInfo.Id
        };
    }

    internal static DisconnectedEvent Disconnect(long connectionId)
    {
        return new()
        {
            EventId = Interlocked.Increment(ref _counter),
            EventTime = Monotonic.Time(),
            ConnectionId = connectionId
        };
    }
}