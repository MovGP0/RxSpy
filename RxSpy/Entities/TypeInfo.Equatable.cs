namespace RxSpy.Entities;

public partial struct TypeInfo : IEquatable<TypeInfo>
{
    [Pure]
    public bool Equals(TypeInfo other)
    {
        return Name == other.Name
               && Namespace == other.Namespace;
    }

    [Pure]
    public override bool Equals(object? obj) => obj is TypeInfo other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Name, Namespace);

    [Pure]
    public static bool operator ==(TypeInfo left, TypeInfo right) => left.Equals(right);

    [Pure]
    public static bool operator !=(TypeInfo left, TypeInfo right) => !left.Equals(right);
}