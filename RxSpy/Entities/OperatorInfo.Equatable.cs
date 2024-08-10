namespace RxSpy.Entities;

public partial struct OperatorInfo : IEquatable<OperatorInfo>
{
    [Pure]
    public bool Equals(OperatorInfo other)
    {
        return Id == other.Id
               && CallSite.Equals(other.CallSite)
               && OperatorMethod.Equals(other.OperatorMethod)
               && Name == other.Name
               && IsAnonymous == other.IsAnonymous;
    }

    [Pure]
    public override bool Equals(object? obj) => obj is OperatorInfo other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Id, CallSite, OperatorMethod, Name, IsAnonymous);

    [Pure]
    public static bool operator ==(OperatorInfo left, OperatorInfo right) => left.Equals(right);

    [Pure]
    public static bool operator !=(OperatorInfo left, OperatorInfo right) => !left.Equals(right);
}