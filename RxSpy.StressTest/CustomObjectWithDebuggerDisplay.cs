using System.Diagnostics;

namespace RxSpy.StressTest;

[DebuggerDisplay("{Name} {Value,nq}")]
public sealed class CustomObjectWithDebuggerDisplay
{
    public string Name { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}