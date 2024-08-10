namespace RxSpy.Events;

public partial struct SubscribeEvent : IEquatable<SubscribeEvent>
{
    [Pure]
    public bool Equals(SubscribeEvent other)
    {
        return EventId == other.EventId
               && EventTime == other.EventTime
               && ChildId == other.ChildId
               && ParentId == other.ParentId;
    }

    [Pure]
    public override bool Equals(object? obj) => obj is SubscribeEvent other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(EventId, EventTime, ChildId, ParentId);

    [Pure]
    public static bool operator ==(SubscribeEvent left, SubscribeEvent right) => left.Equals(right);

    [Pure]
    public static bool operator !=(SubscribeEvent left, SubscribeEvent right) => !left.Equals(right);
}