namespace RxSpy.Events;

public partial struct UnsubscribeEvent : IEquatable<UnsubscribeEvent>
{
    [Pure]
    public bool Equals(UnsubscribeEvent other)
    {
        return EventId == other.EventId
               && EventTime == other.EventTime
               && SubscriptionId == other.SubscriptionId;
    }

    [Pure]
    public override bool Equals(object? obj) => obj is UnsubscribeEvent other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(EventId, EventTime, SubscriptionId);

    [Pure]
    public static bool operator ==(UnsubscribeEvent left, UnsubscribeEvent right) => left.Equals(right);

    [Pure]
    public static bool operator !=(UnsubscribeEvent left, UnsubscribeEvent right) => !left.Equals(right);
}