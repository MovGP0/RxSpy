using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace RxSpy.Utils;

public static class MethodInvoker
{
    private static readonly ConcurrentDictionary<MethodBase, Lazy<Func<object, object[], object>>> _cache = new();

    public static object Invoke(object target, Type targetType, MethodInfo method, object[] args)
    {
        var invokeDelegate = _cache.GetOrAdd(
            method,
            _ => new Lazy<Func<object, object[], object>>(() => CreateInvokeDelegate(targetType, method))
        );

        return invokeDelegate.Value(target, args);
    }

    private static Func<object, object[], object> CreateInvokeDelegate(Type targetType, MethodInfo method)
    {
        var invokeDelegate = new DynamicMethod("InvokeFast_" + method.Name, typeof(object), new[] { typeof(object), typeof(object[]) }, true);
            
        var generator = invokeDelegate.GetILGenerator();

        var parameterTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();

        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Unbox_Any, targetType);

        for (int i = 0; i < parameterTypes.Length; i++)
        {
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldc_I4, i);
            generator.Emit(OpCodes.Ldelem_Ref);

            if (parameterTypes[i] != typeof(object))
                generator.Emit(OpCodes.Unbox_Any, parameterTypes[i]);
        }

        generator.Emit(OpCodes.Callvirt, method);

        if (method.ReturnType.IsValueType)
            generator.Emit(OpCodes.Box, method.ReturnType);

        generator.Emit(OpCodes.Ret);

        return (Func<object, object[], object>)invokeDelegate.CreateDelegate(typeof(Func<object, object[], object>));
    }
}