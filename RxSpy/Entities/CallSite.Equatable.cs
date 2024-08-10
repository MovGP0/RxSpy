namespace RxSpy.Entities;

public partial struct CallSite : IEquatable<CallSite>
{
    [Pure]
    public bool Equals(CallSite other)
    {
        return Line == other.Line
               && File == other.File
               && ILOffset == other.ILOffset
               && Nullable.Equals(Method, other.Method);
    }

    [Pure]
    public override bool Equals(object? obj) => obj is CallSite other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Line, File, ILOffset, Method);

    [Pure]
    public static bool operator ==(CallSite left, CallSite right) => left.Equals(right);

    [Pure]
    public static bool operator !=(CallSite left, CallSite right) => !left.Equals(right);
}