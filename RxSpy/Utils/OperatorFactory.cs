using System.Collections.Concurrent;
using System.Reflection;
using RxSpy.Observables;
using RxSpy.Protobuf.Events;

namespace RxSpy.Utils;

public static class OperatorFactory
{
    private readonly static ConcurrentDictionary<Type, Lazy<ConstructorInfo>> _connectionConstructorCache = new();

    public static object CreateOperatorObservable(object source, Type signalType, OperatorInfo operatorInfo)
    {
        var ctor = _connectionConstructorCache.GetOrAdd(
            signalType,
            _ => new Lazy<ConstructorInfo>(() => GetOperatorConstructor(signalType)));

        return ctor.Value.Invoke(new object[] { RxSpySession.Current, source, operatorInfo });
    }

    private static ConstructorInfo GetOperatorConstructor(Type signalType)
    {
        var operatorObservable = typeof(OperatorObservable<>).MakeGenericType(signalType);

        return operatorObservable.GetConstructor(new[] { 
            typeof(RxSpySession), 
            typeof(IObservable<>).MakeGenericType(signalType), 
            typeof(OperatorInfo) 
        });
    }
}