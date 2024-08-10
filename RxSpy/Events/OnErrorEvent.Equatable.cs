namespace RxSpy.Events;

public partial struct OnErrorEvent : IEquatable<OnErrorEvent>
{
    [Pure]
    public bool Equals(OnErrorEvent other)
    {
        return EventId == other.EventId
               && EventTime == other.EventTime
               && ErrorType.Equals(other.ErrorType)
               && Message == other.Message
               && OperatorId == other.OperatorId
               && StackTrace == other.StackTrace;
    }

    [Pure]
    public override bool Equals(object? obj) => obj is OnErrorEvent other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(EventId, EventTime, ErrorType, Message, OperatorId, StackTrace);

    [Pure]
    public static bool operator ==(OnErrorEvent left, OnErrorEvent right) => left.Equals(right);

    [Pure]
    public static bool operator !=(OnErrorEvent left, OnErrorEvent right) => !left.Equals(right);
}