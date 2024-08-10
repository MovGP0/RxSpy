namespace RxSpy.Events;

public partial struct TagOperatorEvent : IEquatable<TagOperatorEvent>
{
    [Pure]
    public bool Equals(TagOperatorEvent other)
    {
        return EventId == other.EventId
               && EventTime == other.EventTime
               && OperatorId == other.OperatorId
               && Tag == other.Tag;
    }

    [Pure]
    public override bool Equals(object? obj) => obj is TagOperatorEvent other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(EventId, EventTime, OperatorId, Tag);

    [Pure]
    public static bool operator ==(TagOperatorEvent left, TagOperatorEvent right) => left.Equals(right);

    [Pure]
    public static bool operator !=(TagOperatorEvent left, TagOperatorEvent right) => !left.Equals(right);
}