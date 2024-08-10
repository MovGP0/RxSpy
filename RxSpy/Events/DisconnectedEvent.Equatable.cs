namespace RxSpy.Events;

public partial struct DisconnectedEvent : IEquatable<DisconnectedEvent>
{
    [Pure]
    public bool Equals(DisconnectedEvent other)
    {
        return EventId == other.EventId
               && EventTime == other.EventTime
               && ConnectionId == other.ConnectionId;
    }

    [Pure]
    public override bool Equals(object? obj) => obj is DisconnectedEvent other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(EventId, EventTime, ConnectionId);

    [Pure]
    public static bool operator ==(DisconnectedEvent left, DisconnectedEvent right) => left.Equals(right);

    [Pure]
    public static bool operator !=(DisconnectedEvent left, DisconnectedEvent right) => !left.Equals(right);
}