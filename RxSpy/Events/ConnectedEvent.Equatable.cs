namespace RxSpy.Events;

public partial struct ConnectedEvent : IEquatable<ConnectedEvent>
{
    [Pure]
    public bool Equals(ConnectedEvent other)
    {
        return EventId == other.EventId
               && EventTime == other.EventTime
               && OperatorId == other.OperatorId;
    }

    [Pure]
    public override bool Equals(object? obj) => obj is ConnectedEvent other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(EventId, EventTime, OperatorId);

    [Pure]
    public static bool operator ==(ConnectedEvent left, ConnectedEvent right) => left.Equals(right);

    [Pure]
    public static bool operator !=(ConnectedEvent left, ConnectedEvent right) => !left.Equals(right);
}