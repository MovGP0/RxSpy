using freakcode.frequency;
using Google.Protobuf.WellKnownTypes;
using RxSpy.Protobuf.Events;
using RxSpy.Utils;
using Type = System.Type;

namespace RxSpy.Events;

internal abstract class EventFactory
{
    private static long _counter;

    public static OperatorCreatedEvent OperatorCreated(OperatorInfo operatorInfo)
    {
        return new()
        {
            BaseEvent = GetBaseEvent(EventType.OperatorCreated),
            CallSite = operatorInfo.CallSite,
            Id = operatorInfo.Id,
            Name = operatorInfo.Name,
            OperatorMethod = operatorInfo.OperatorMethod
        };
    }

    public static OnNextEvent OnNext(OperatorInfo operatorInfo, Type valueType, object value)
    {
        return new()
        {
            OperatorId = operatorInfo.Id,
            BaseEvent = GetBaseEvent(EventType.OnNext),
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
                OperatorId = operatorInfo.Id,
                BaseEvent = GetBaseEvent(EventType.OnError)
            };
        }

        return new()
        {
            OperatorId = operatorInfo.Id,
            ErrorType = TypeInfoFactory.Create(error.GetType()),
            Message = error.Message,
            StackTrace = error.StackTrace,
            BaseEvent = GetBaseEvent(EventType.OnError)
        };
    }

    public static OnCompletedEvent OnCompleted(OperatorInfo operatorInfo)
    {
        return new()
        {
            OperatorId = operatorInfo.Id,
            BaseEvent = GetBaseEvent(EventType.OnCompleted)
        };
    }

    private static Event GetBaseEvent(EventType eventType)
    {
        var timeInMilliseconds = Monotonic.Time();
        return new()
        {
            EventType = eventType,
            EventTime = new()
            {
                Seconds = timeInMilliseconds / 1000L,
                Nanos = (int) (timeInMilliseconds % 1000L) * 1_000
            },
            EventId = Interlocked.Increment(ref _counter)
        };
    }

    internal static SubscribeEvent Subscribe(OperatorInfo child, OperatorInfo parent)
    {
        return new()
        {
            ChildId = child.Id,
            ParentId = parent.Id,
            BaseEvent = GetBaseEvent(EventType.Subscribe)
        };
    }

    internal static UnsubscribeEvent Unsubscribe(long subscriptionId)
    {
        return new()
        {
            SubscriptionId = subscriptionId,
            BaseEvent = GetBaseEvent(EventType.Unsubscribe)
        };
    }

    internal static TagOperatorEvent Tag(OperatorInfo operatorInfo, string tag)
    {
        return new()
        {
            BaseEvent = GetBaseEvent(EventType.TagOperator),
            OperatorId = operatorInfo.Id,
            Tag = tag
        };
    }

    internal static ConnectedEvent Connect(OperatorInfo operatorInfo)
    {
        return new()
        {
            OperatorId = operatorInfo.Id,
            BaseEvent = GetBaseEvent(EventType.Connected)
        };
    }

    internal static DisconnectedEvent Disconnect(long connectionId)
    {
        return new()
        {
            ConnectionId = connectionId,
            BaseEvent = GetBaseEvent(EventType.Disconnected)
        };
    }
}