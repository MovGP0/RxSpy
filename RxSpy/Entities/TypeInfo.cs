using System.Diagnostics;

namespace RxSpy.Entities;

[DebuggerDisplay("{DebuggerDisplay}")]
public partial struct TypeInfo
{
    public string Name { get; init; }
    public string? Namespace { get; init; }

    [Pure]
    private string DebuggerDisplay => Namespace is not null ? $"{Namespace}.{Name}" : Name;
}