namespace RxSpy.Events;

public partial struct OnNextEvent : IEquatable<OnNextEvent>
{
    [Pure]
    public bool Equals(OnNextEvent other)
    {
        return EventId == other.EventId
               && EventTime == other.EventTime
               && OperatorId == other.OperatorId
               && ValueType == other.ValueType
               && Value == other.Value
               && Thread == other.Thread;
    }

    [Pure]
    public override bool Equals(object? obj) => obj is OnNextEvent other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(EventId, EventTime, OperatorId, ValueType, Value, Thread);

    [Pure]
    public static bool operator ==(OnNextEvent left, OnNextEvent right) => left.Equals(right);

    [Pure]
    public static bool operator !=(OnNextEvent left, OnNextEvent right) => !left.Equals(right);
}