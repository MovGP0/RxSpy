using System.Diagnostics;

namespace RxSpy.AspNet.TestConsole;

[DebuggerDisplay("{foo,nq}")]
internal sealed class Dummy
{
    string foo = "bar";
}