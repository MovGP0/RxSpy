namespace RxSpy.Entities;

public partial struct MethodInfo : IEquatable<MethodInfo>
{
    [Pure]
    public bool Equals(MethodInfo other)
    {
        return Namespace == other.Namespace
               && DeclaringType == other.DeclaringType
               && Name == other.Name
               && Signature == other.Signature;
    }

    [Pure]
    public override bool Equals(object? obj) => obj is MethodInfo other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Namespace, DeclaringType, Name, Signature);

    [Pure]
    public static bool operator ==(MethodInfo left, MethodInfo right) => left.Equals(right);

    [Pure]
    public static bool operator !=(MethodInfo left, MethodInfo right) => !left.Equals(right);
}