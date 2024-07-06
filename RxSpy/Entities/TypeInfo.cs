using System.Diagnostics;

namespace RxSpy.Entities;

[DebuggerDisplay("{DebuggerDisplay}")]
public struct TypeInfo
{
    public string Name { get; init; }
    public string? Namespace { get; init; }

    private string DebuggerDisplay => Namespace is not null ? $"{Namespace}.{Name}" : Name;
}