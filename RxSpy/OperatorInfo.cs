using RxSpy.Protobuf.Events;

namespace RxSpy;

public static class OperatorInfoFactory
{
    private static long _idCounter;

    internal static OperatorInfo Create(CallSite callSite, MethodInfo operatorMethod)
    {
        return new OperatorInfo
        {
            Id = Interlocked.Increment(ref _idCounter),
            CallSite = callSite,
            OperatorMethod = operatorMethod,
            Name = operatorMethod.Name,
            IsAnonymous = false
        };
    }

    internal static OperatorInfo Create(string name)
    {
        return new OperatorInfo
        {
            Id = Interlocked.Increment(ref _idCounter),
            Name = name,
            IsAnonymous = true
        };
    }

    public static string ToString(OperatorInfo operatorInfo)
        => operatorInfo.Name + "#" + operatorInfo.Id;
}