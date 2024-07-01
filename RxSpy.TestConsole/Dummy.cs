using System.Diagnostics;

namespace RxSpy.TestConsole;

[DebuggerDisplay("{foo,nq}")]
internal sealed class Dummy
{
    string foo = "bar";
}