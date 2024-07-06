using System.Diagnostics;
using RxSpy.Entities;

namespace RxSpy.Factories;

public static class CallSiteFactory
{
    public static CallSite Create(StackFrame? frame)
    {
        if (frame is null)
        {
            return new CallSite();
        }

        var method = frame.GetMethod();
        return new()
        {
            Line = frame.GetFileLineNumber(),
            File = frame.GetFileName(),
            ILOffset = frame.GetILOffset(),
            Method = method != null ? MethodInfoFactory.Create(method) : null
        };
    }
}