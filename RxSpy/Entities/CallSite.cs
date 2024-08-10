using System.Diagnostics;

namespace RxSpy.Entities;

[DebuggerDisplay("@{File}:{Line}\n{Method}")]
public partial struct CallSite
{
    public int Line { get; init; }
    public string? File { get; init; }
    public int ILOffset { get; init; }
    public MethodInfo? Method { get; init; }
}