using System.Reflection;
using System.Reflection.Emit;

namespace RxSpy.Interception;

public static class ProxyFactory
{
    /// <summary>
    /// Creates a proxy object that intercepts calls to the given interface.
    /// If the target object implements the methods of the interface, the proxy will call the target object.
    /// Then the proxy will call the original implementation.
    /// </summary>
    /// <param name="interface">The interface to intercept</param>
    /// <param name="spyObject">
    /// The object where calls get redirected to.
    /// May not implement all methods supported by the interface.</param>
    /// <param name="originalObject">The original object to be called</param>
    /// <returns></returns>
    public static object? CreateProxy(
        Type @interface,
        object spyObject,
        object originalObject)
    {
        var originalImplType = originalObject.GetType();

        if (originalObject.GetType().GetInterfaces().All(i => i != @interface))
        {
            throw new InvalidOperationException("Original implementation does not implement the internal interface");
        }

        var targetType = spyObject.GetType();
        var assemblyName = new AssemblyName("RxSpy.DynamicProxyAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        var typeBuilder = moduleBuilder.DefineType("InternalProxy", TypeAttributes.Public | TypeAttributes.Class);
        typeBuilder.AddInterfaceImplementation(@interface);

        // Define the private fields
        var targetField = typeBuilder.DefineField("_target", targetType, FieldAttributes.Private);
        var originalImplField = typeBuilder.DefineField("_originalImpl", originalImplType, FieldAttributes.Private);

        // Implement the constructor
        var constructorBuilder = typeBuilder.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.Standard,
            [targetType, originalImplType]);

        // use the constructor to set the private fields
        var ilConstructorGenerator = constructorBuilder.GetILGenerator();
        ilConstructorGenerator.Emit(OpCodes.Ldarg_0);
        ilConstructorGenerator.Emit(OpCodes.Ldarg_1);
        ilConstructorGenerator.Emit(OpCodes.Stfld, targetField);
        ilConstructorGenerator.Emit(OpCodes.Ldarg_0);
        ilConstructorGenerator.Emit(OpCodes.Ldarg_2);
        ilConstructorGenerator.Emit(OpCodes.Stfld, originalImplField);
        ilConstructorGenerator.Emit(OpCodes.Ret);

        // implement the methods of the interface
        foreach (var method in @interface.GetMethods())
        {
            var parameters = method.GetParameters();
            var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

            var methodBuilder = typeBuilder.DefineMethod(
                method.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                method.ReturnType,
                parameterTypes);

            var ilGenerator = methodBuilder.GetILGenerator();

            // Call the method on the target object
            var methodToCall = targetType.GetMethod(method.Name, parameterTypes);
            if (methodToCall is not null)
            {
                // Load the target object (the interceptor)
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldfld, targetField);

                // Load the arguments
                for (var i = 0; i < parameters.Length; i++)
                {
                    ilGenerator.Emit(OpCodes.Ldarg, i + 1);
                }

                ilGenerator.Emit(OpCodes.Callvirt, methodToCall);
            }

            var originalMethod = originalImplType.GetMethod(method.Name, parameterTypes);
            if (originalMethod is not null)
            {
                // Load the original implementation object
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldfld, originalImplField);

                // Load the arguments
                for (var i = 0; i < parameters.Length; i++)
                {
                    ilGenerator.Emit(OpCodes.Ldarg, i + 1);
                }

                ilGenerator.Emit(OpCodes.Callvirt, originalMethod);
                
                if (method.ReturnType != typeof(void))
                {
                    ilGenerator.DeclareLocal(method.ReturnType);
                    ilGenerator.Emit(OpCodes.Stloc_0);
                }
            }

            // return
            ilGenerator.Emit(OpCodes.Ret);
        }

        var proxyType = typeBuilder.CreateType();
        var proxyInstance = Activator.CreateInstance(proxyType, spyObject, originalObject);
        return proxyInstance;
    }
}