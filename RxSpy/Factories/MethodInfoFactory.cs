using System.Reflection;
using RxSpy.Utils;

namespace RxSpy.Factories;

public static class MethodInfoFactory
{
    public static RxSpy.Entities.MethodInfo Create(MethodBase method)
    {
        var name = GetName(method);
        return new RxSpy.Entities.MethodInfo
        {
            Namespace = method.Name,
            DeclaringType = method.DeclaringType?.Name,
            Name = name,
            Signature = name + " (" + GetArguments(method) + ")"
        };
    }

    private static string GetName(MethodBase method)
    {
        if (method.IsGenericMethod)
        {
            var genericArgs = method.GetGenericArguments();
            return method.Name + "<" + string.Join(", ", genericArgs.Select(TypeUtils.ToFriendlyName)) + ">";
        }

        return method.Name;
    }

    private static string GetArguments(MethodBase method)
    {
        var arguments = new List<string>();

        foreach (var arg in method.GetParameters())
        {
            arguments.Add(GetArgument(arg));
        }

        return string.Join(", ", arguments);
    }

    private static string GetArgument(ParameterInfo arg)
    {
        if (arg.ParameterType.IsGenericParameter)
        {
        }

        return TypeUtils.ToFriendlyName(arg.ParameterType) + " " + arg.Name;
    }
}