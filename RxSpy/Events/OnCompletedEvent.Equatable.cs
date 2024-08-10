namespace RxSpy.Events;

public partial struct OnCompletedEvent : IEquatable<OnCompletedEvent>
{
    [Pure]
    public bool Equals(OnCompletedEvent other)
    {
        return EventId == other.EventId
               && EventTime == other.EventTime
               && OperatorId == other.OperatorId;
    }

    [Pure]
    public override bool Equals(object? obj) => obj is OnCompletedEvent other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(EventId, EventTime, OperatorId);

    [Pure]
    public static bool operator ==(OnCompletedEvent left, OnCompletedEvent right) => left.Equals(right);

    [Pure]
    public static bool operator !=(OnCompletedEvent left, OnCompletedEvent right) => !left.Equals(right);
}