using System.Diagnostics;
using RxSpy.Protobuf.Events;

namespace RxSpy.Events;

public static class CallSiteFactory
{
    public static CallSite Create(StackFrame frame)
    {
        var method = frame.GetMethod();
        return new()
        {
            Line = frame.GetFileLineNumber(),
            File = frame.GetFileName(),
            IlOffset = frame.GetILOffset(),
            Method = method != null ? MethodInfoFactory.Create(method) : null
        };
    }
}