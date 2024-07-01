using System.Collections.Concurrent;
using System.Reflection;
using RxSpy.Observables;
using RxSpy.Protobuf.Events;

namespace RxSpy.Utils;

public static class ConnectionFactory
{
    private readonly static ConcurrentDictionary<Type, Lazy<Func<object, OperatorInfo, object>>> _connectionFactoryCache =
        new();

    private readonly static ConcurrentDictionary<Type, Lazy<ConstructorInfo>> _connectionConstructorCache = new();

    public static bool TryCreateConnection(Type type, object value, OperatorInfo operatorInfo, out object connectionObject)
    {
        var factory = _connectionFactoryCache.GetOrAdd(
            type,
            _ => new Lazy<Func<object, OperatorInfo, object>>(
                () => CreateConnectionFactory(type),
                LazyThreadSafetyMode.ExecutionAndPublication)
        );

        if (factory.Value == null)
        {
            connectionObject = null;
            return false;
        }

        connectionObject = factory.Value(value, operatorInfo);
        return true;
    }

    private static Func<object, OperatorInfo, object> CreateConnectionFactory(Type pt)
    {
        if (IsGenericTypeDefinition(pt, typeof(IObservable<>)))
        {
            var signalType = pt.GetGenericArguments()[0];

            return (value, operatorInfo) => TryCreateObservableConnection(value, signalType, operatorInfo);
        }
        else if (pt.IsArray)
        {
            var observableType = pt.GetElementType();

            if (!IsGenericTypeDefinition(observableType, typeof(IObservable<>)))
            {
                return null;
            }

            var signalType = observableType.GetGenericArguments()[0];

            return (value, operatorInfo) =>
            {
                var argArray = (Array)value;
                var newArray = Array.CreateInstance(observableType, argArray.Length);

                for (int i = 0; i < argArray.Length; i++)
                {
                    newArray.SetValue(TryCreateObservableConnection(argArray.GetValue(i), signalType, operatorInfo), i);
                }

                return newArray;
            };
        }
        else if (IsGenericTypeDefinition(pt, typeof(IEnumerable<>)) &&
                 IsGenericTypeDefinition(pt.GetGenericArguments()[0], typeof(IObservable<>)))
        {
            var observableType = pt.GetGenericArguments()[0];
            var signalType = observableType.GetGenericArguments()[0];

            var enumerableConnectionType = typeof(DeferredOperatorConnectionEnumerable<>)
                .MakeGenericType(observableType);

            return (value, operatorInfo) =>
            {
                return Activator.CreateInstance(
                    enumerableConnectionType,
                    new object[] { 
                        value, 
                        new Func<object, object>(o => TryCreateObservableConnection(o, signalType, operatorInfo)) 
                    });
            };
        }

        return null;
    }

    private static object TryCreateObservableConnection(object source, Type signalType, OperatorInfo operatorInfo)
    {
        if (source is not IOperatorObservable)
            return source;

        var ctor = _connectionConstructorCache.GetOrAdd(
            signalType, 
            _ => new Lazy<ConstructorInfo>(() => GetConnectionConstructor(signalType)));

        return ctor.Value.Invoke(new object[] { RxSpySession.Current, source, operatorInfo });
    }

    private static ConstructorInfo GetConnectionConstructor(Type signalType)
    {
        var operatorObservable = typeof(OperatorConnection<>).MakeGenericType(signalType);

        return operatorObservable.GetConstructor(new[] { typeof(RxSpySession), typeof(IObservable<>).MakeGenericType(signalType), typeof(OperatorInfo) });
    }

    private static bool IsGenericTypeDefinition(Type source, Type genericTypeComparand)
    {
        return source.IsGenericType && source.GetGenericTypeDefinition() == genericTypeComparand;
    }

    private class DeferredOperatorConnectionEnumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _source;
        private readonly Func<object, object> _selector;

        public DeferredOperatorConnectionEnumerable(IEnumerable<T> source, Func<object, object> selector)
        {
            _source = source;
            _selector = selector;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _source)
                yield return (T)_selector(item);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}