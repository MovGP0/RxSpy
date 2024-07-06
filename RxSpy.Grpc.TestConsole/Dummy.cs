using System.Diagnostics;

namespace RxSpy.Grpc.TestConsole;

[DebuggerDisplay("{foo,nq}")]
internal sealed class Dummy
{
    string foo = "bar";
}