using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace RxSpy.Factories;

internal static class StackFrameFactory
{
    [Pure]
    internal static StackFrame? GetForMethod(MethodInfo info)
    {
        var stackTrace = new StackTrace(true);
        var frames = stackTrace.GetFrames();
        if (frames is null)
        {
            return null;
        }

        foreach (var frame in frames)
        {
            var method = frame.GetMethod();
            if (method == info)
            {
                return frame;
            }
        }

        return null;
    }

    [Pure]
    internal static StackFrame? GetForObject(object? obj)
    {
        if (obj is null)
        {
            return null;
        }

        var stackTrace = new StackTrace(true);

        StackFrame? stackFrame = null;
        var frames = stackTrace.GetFrames();
        if (frames is null)
        {
            return null;
        }

        foreach (var frame in frames)
        {
            var method = frame.GetMethod();
            if (method != null && method.GetParameters().Length > 0)
            {
                // Check if any parameter of the method matches the argument object type
                foreach (var param in method.GetParameters())
                {
                    if (obj != null && param.ParameterType == obj.GetType())
                    {
                        stackFrame = frame;
                        break;
                    }
                }
            }

            if (stackFrame != null)
            {
                break;
            }
        }

        return stackFrame;
    }
}