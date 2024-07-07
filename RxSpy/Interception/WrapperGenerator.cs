using System.Reflection;
using System.Reflection.Emit;

namespace RxSpy.Interception;

public static class QueryLanguageWrapperGenerator
{
    private static Type? _wrapperType;
    private static readonly object LockObject = new();

    /// <summary>
    /// Creates a dynamic proxy implementing the <see cref="IQueryLanguage"/> interface.
    /// </summary>
    /// <param name="targetInstance">The object to wrap. Must implement <see cref="System.Reactive.Linq.IQueryLanguage"/></param>
    /// <param name="spy">The spy object that records the interface calls.</param>
    /// <returns>An object implementing both, <see cref="System.Reactive.Linq.IQueryLanguage"/> and <see cref="IQueryLanguage"/></returns>
    internal static object Create(object targetInstance, IQueryLanguage spy)
    {
        // check if targetInstance implement interfaceType
        var targetType = targetInstance.GetType();

        var assembly = Assembly.Load(new AssemblyName("System.Reactive.Linq"));
        var targetInterface = assembly.GetType("System.Reactive.Linq.IQueryLanguage");
        if (targetInterface is null)
        {
            throw new InvalidOperationException("The type System.Reactive.Linq.IQueryLanguage could not be loaded");
        }

        if (!targetInterface.IsAssignableFrom(targetType))
        {
            throw new InvalidOperationException($"The target instance does not implement {targetInterface}");
        }

        // lock to ensure only one instance is created
        lock (LockObject)
        {
            if (_wrapperType is not null)
            {
                return Activator.CreateInstance(_wrapperType, targetInstance, spy)!;
            }

            _wrapperType = CreateWrapper();
        }

        // Create an instance of the wrapper type
        var wrapperInstance = Activator.CreateInstance(_wrapperType, targetInstance, spy);
        return wrapperInstance!;
    }

    private static Type CreateWrapper()
    {
        var assembly = Assembly.Load(new AssemblyName("System.Reactive.Linq"));

        var targetInterface = assembly.GetType("System.Reactive.Linq.IQueryLanguage");
        if (targetInterface is null)
        {
            throw new InvalidOperationException("The type System.Reactive.Linq.IQueryLanguage could not be loaded");
        }

        var spyInterface = typeof(IQueryLanguage);

        // Create a dynamic assembly and module
        var typeBuilder = CreateQueryLanguageWrapper(targetInterface);

        // Define a field to hold the private class instance
        var targetField = typeBuilder.DefineField("_target", targetInterface, FieldAttributes.Private);
        var spyField = typeBuilder.DefineField("_spy", spyInterface, FieldAttributes.Private);

        typeBuilder.AddConstructor(targetInterface, targetField, spyInterface, spyField);
        typeBuilder.AddMethods(targetInterface, targetField, spyInterface, spyField);

        // Create the wrapper type
        var wrapperType = typeBuilder.CreateType();
        return wrapperType;
    }

    /// <summary>
    /// Creates a dynamic assembly named <c>RxSpy.DynamicProxyAssembly</c>
    /// with the following wrapper type:
    /// <code>
    /// namespace RxSpy
    /// {
    ///     public sealed class QueryLanguageWrapper:
    ///         System.Reactive.Linq.IQueryLanguage
    ///     {
    ///     }
    /// }
    /// </code> 
    /// </summary>
    private static TypeBuilder CreateQueryLanguageWrapper(Type targetInterface)
    {
        var assemblyName = new AssemblyName("RxSpy.DynamicProxyAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("RxSpy");

        return moduleBuilder.DefineType(
            "QueryLanguageWrapper",
            TypeAttributes.Public | TypeAttributes.Sealed,
            typeof(object),
            [targetInterface]);
    }

    /// <summary>
    /// Creates the following constructor for the dynamic wrapper type:
    /// <code>
    /// QueryLanguageWrapper(
    ///     System.Reactive.Linq.IQueryLanguage target,
    ///     RxSpy.IQueryLanguage spy)
    /// {
    ///     _target = target;
    ///     _spy = spy;
    /// }
    /// </code>
    /// </summary>
    private static void AddConstructor(
        this TypeBuilder typeBuilder,
        Type targetType,
        FieldBuilder targetField,
        Type spyType,
        FieldBuilder spyField)
    {
        // create constructor method
        var constructorBuilder = typeBuilder.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.Standard,
            [targetType, spyType]);

        var ilGenerator = constructorBuilder.GetILGenerator();

        // call base constructor
        ilGenerator.Emit(OpCodes.Ldarg_0);
        var baseConstructor = typeof(object).GetConstructor(Type.EmptyTypes);
        if (baseConstructor is null)
        {
            throw new InvalidOperationException("Base constructor of type System.Object not found");
        }
        ilGenerator.Emit(OpCodes.Call, baseConstructor);

        // set _target field
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldarg_1);
        ilGenerator.Emit(OpCodes.Stfld, targetField);

        // set _spy field
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldarg_2);
        ilGenerator.Emit(OpCodes.Stfld, spyField);

        // return
        ilGenerator.Emit(OpCodes.Ret);
    }

    /// <summary>
    /// Implements the methods of the target interface and the spy interface.
    /// </summary>
    private static void AddMethods(
        this TypeBuilder typeBuilder,
        Type targetInterface,
        FieldBuilder targetField,
        Type spyInterface,
        FieldBuilder spyField)
    {
        foreach (var targetMethod in targetInterface.GetMethods())
        {
            var methodParameters = targetMethod.GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();

            var spyMethod = spyInterface.GetMethod(targetMethod.Name, methodParameters);
            if (spyMethod == null)
            {
                // Method is not defined in the proxy interface
                typeBuilder.ImplementTransparentInternalMethod(
                    targetInterface,
                    targetField,
                    targetMethod);
            }
            else
            {
                // Method is defined in both interfaces
                typeBuilder.ImplementProxyInternalMethod(
                    targetInterface,
                    targetField,
                    targetMethod,
                    spyInterface,
                    spyField,
                    spyMethod);
            }
        }
    }

    /// <summary>
    /// Implements a method that calls the target method.
    /// </summary>
    /// <remarks>
    /// Uses implicit interface implementation.
    /// </remarks>
    /// <example>
    /// <code>
    /// public TReturn MethodName(TArg1 arg1, TArg2 arg2)
    /// {
    ///     try
    ///     {
    ///         SpyInterface spy = (SpyInterface)_spy;
    ///         _ = spy.MethodName(arg1, arg2);
    ///     }
    ///     finally
    ///     {
    ///         TargetInterface target = _target;
    ///         var result = target.MethodName(arg1, arg2);
    ///         return result;
    ///     }
    /// }
    /// </code>
    /// </example>
    private static void ImplementProxyInternalMethod(
        this TypeBuilder typeBuilder,
        Type targetInterface,
        FieldBuilder targetField,
        MethodInfo targetMethod,
        Type spyInterface,
        FieldBuilder spyField,
        MethodInfo spyMethod)
    {
        // Define method parameters and return type
        var parameterTypes = targetMethod.GetParameters()
            .Select(p => p.ParameterType)
            .ToArray();

        var returnType = targetMethod.ReturnType;
        var methodBuilder = typeBuilder.DefineMethod(
            targetMethod.Name,
            MethodAttributes.Public,
            returnType,
            parameterTypes);

        // Generate IL for the method
        var ilGenerator = methodBuilder.GetILGenerator();

        // Begin try block
        var tryBlock = ilGenerator.BeginExceptionBlock();

        // Load the spy field (_spy) onto the evaluation stack
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldfld, spyField);
        ilGenerator.Emit(OpCodes.Castclass, spyInterface);

        // Load method arguments onto the evaluation stack for the spy method
        for (int i = 0; i < parameterTypes.Length; i++)
        {
            ilGenerator.Emit(OpCodes.Ldarg, i + 1);
        }

        // Call the spy method
        ilGenerator.Emit(OpCodes.Callvirt, spyMethod);

        // Pop the result if the spy method has a return type (to discard it)
        if (spyMethod.ReturnType != typeof(void))
        {
            ilGenerator.Emit(OpCodes.Pop);
        }

        // Begin finally block
        ilGenerator.BeginFinallyBlock();

        // Load the target field (_target) onto the evaluation stack
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldfld, targetField);

        // Load method arguments onto the evaluation stack for the target method
        for (int i = 0; i < parameterTypes.Length; i++)
        {
            ilGenerator.Emit(OpCodes.Ldarg, i + 1);
        }

        // Call the target method
        ilGenerator.Emit(OpCodes.Callvirt, targetMethod);

        // Return the result (if any)
        ilGenerator.Emit(returnType == typeof(void) ? OpCodes.Nop : OpCodes.Ret);

        // End exception block
        ilGenerator.EndExceptionBlock();

        // Complete the method
        ilGenerator.Emit(OpCodes.Ret);

        // Implement the method on the type
        typeBuilder.DefineMethodOverride(methodBuilder, targetMethod);
    }

    /// <summary>
    /// Implement method that calls the _target instance directly.
    /// </summary>
    /// <remarks>Uses implicit interface implementation.</remarks>
    /// <example>
    /// <code>
    /// public TReturn MethodName(TArg1 arg1, TArg2 arg2)
    /// {
    ///     TargetInterface target = _target;
    ///     var result = target.MethodName(arg1, arg2);
    ///     return result;
    /// }
    /// </code>
    /// </example>
    private static void ImplementTransparentInternalMethod(
        this TypeBuilder typeBuilder,
        Type targetInterface,
        FieldBuilder targetField,
        MethodInfo targetMethod)
    {
        // Define method parameters and return type
        var parameterTypes = targetMethod.GetParameters()
            .Select(p => p.ParameterType)
            .ToArray();

        var returnType = targetMethod.ReturnType;
        var methodBuilder = typeBuilder.DefineMethod(
            targetMethod.Name,
            MethodAttributes.Public,
            returnType,
            parameterTypes);

        // Generate IL for the method
        var ilGenerator = methodBuilder.GetILGenerator();

        // Load the target field (_target) onto the evaluation stack
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldfld, targetField);

        // Load method arguments onto the evaluation stack
        for (int i = 0; i < parameterTypes.Length; i++)
        {
            ilGenerator.Emit(OpCodes.Ldarg, i + 1);
        }

        // Call the target method
        ilGenerator.Emit(OpCodes.Callvirt, targetMethod);

        // Return the result (if any)
        ilGenerator.Emit(returnType == typeof(void) ? OpCodes.Nop : OpCodes.Ret);

        // Complete the method
        ilGenerator.Emit(OpCodes.Ret);

        // Implement the method on the type
        typeBuilder.DefineMethodOverride(methodBuilder, targetMethod);
    }
}