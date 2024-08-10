using System.Diagnostics;

namespace RxSpy.Entities;

[DebuggerDisplay("{Namespace}.{DeclaringType}.{Name}{Signature}")]
public partial struct MethodInfo
{
    public string Namespace { get; init; }
    public string? DeclaringType { get; init; }
    public string Name { get; init; }
    public string Signature { get; init; }
}