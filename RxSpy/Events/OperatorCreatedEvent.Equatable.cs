namespace RxSpy.Events;

public partial struct OperatorCreatedEvent : IEquatable<OperatorCreatedEvent>
{
    [Pure]
    public bool Equals(OperatorCreatedEvent other)
    {
        return EventId == other.EventId
               && EventTime == other.EventTime
               && Id == other.Id
               && Name == other.Name
               && CallSite.Equals(other.CallSite)
               && OperatorMethod.Equals(other.OperatorMethod);
    }

    [Pure]
    public override bool Equals(object? obj) => obj is OperatorCreatedEvent other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(EventId, EventTime, Id, Name, CallSite, OperatorMethod);

    [Pure]
    public static bool operator ==(OperatorCreatedEvent left, OperatorCreatedEvent right) => left.Equals(right);

    [Pure]
    public static bool operator !=(OperatorCreatedEvent left, OperatorCreatedEvent right) => !left.Equals(right);
}