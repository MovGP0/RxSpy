using System.Diagnostics;

namespace RxSpy.Entities;

[DebuggerDisplay("{Name}#{Id}")]
public partial struct OperatorInfo
{
    public long Id { get; init; }
    public CallSite CallSite { get; init; }
    public MethodInfo OperatorMethod { get; init; }
    public string Name { get; init; }
    public bool IsAnonymous { get; init; }

    [Pure]
    public override string ToString() => Name + "#" + Id;
}