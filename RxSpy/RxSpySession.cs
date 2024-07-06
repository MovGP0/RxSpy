using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Castle.DynamicProxy;
using RxSpy.Interception;

namespace RxSpy;

public sealed partial class RxSpySession : IRxSpySession
{
    private static int _launched;
    private readonly IRxSpyEventHandler _eventHandler;
    private readonly bool _explicitCapture;
    private bool _isInCaptureScope;

    public bool IsCapturing => !_explicitCapture || _isInCaptureScope;

    private RxSpySession(IRxSpyEventHandler eventHandler, bool explicitCapture)
    {
        _eventHandler = eventHandler;
        _explicitCapture = explicitCapture;
    }

    public static void LaunchGui(
        Uri address,
        string? pathToRxSpy = null)
    {
        if (_launched == 1)
        {
            throw new InvalidOperationException("Session already created");
        }

        RxSpyGuiLauncher.LaunchGui(address, pathToRxSpy);
    }

    public static RxSpySession Launch(
        IRxSpyEventHandler eventHandler,
        bool explicitCapture = false)
    {
        var session = new RxSpySession(eventHandler, explicitCapture);

        if (Interlocked.CompareExchange(ref _launched, 1, 0) != 0)
            throw new InvalidOperationException("Session already created");

        InstallInterceptingQueryLanguage(session);

        return session;
    }

    private static void InstallInterceptingQueryLanguage(RxSpySession session)
    {
        // Make sure the assembly is initialized
        _ = Assembly.Load(new AssemblyName("System.Reactive.Linq"));
        Observable.Empty<Unit>();

        var observableType = typeof(Observable);
        var systemReactiveLinq = observableType.Assembly;
        var iQueryLanguage = systemReactiveLinq.GetType("System.Reactive.Linq.IQueryLanguage");
        if (iQueryLanguage is null)
        {
            // not supported
            return;
        }

        FieldInfo? defaultImplementationField = observableType.GetField("s_impl", BindingFlags.Static | BindingFlags.NonPublic);
        if (defaultImplementationField is null)
        {
            // not supported
            return;
        }

        object? originalImpl = defaultImplementationField.GetValue(null);
        if (originalImpl is null)
        {
            // not supported
            return;
        }

        var proxyGenerator = new ProxyGenerator();

        // Create a proxy for the public interface
        var publicProxy = new QueryLanguageProxy(originalImpl);

        // Intercept calls to the public proxy
        IInterceptor interceptor = new QueryLanguageInterceptor(session);
        var proxy = proxyGenerator.CreateInterfaceProxyWithTarget<IPublicQueryLanguage>(publicProxy, interceptor);

        if (proxy is null)
        {
            return;
        }

        // Use dynamic assembly generation to create a type that implements the internal interface
        var internalProxy = CreateInternalProxy(iQueryLanguage, proxy);

        defaultImplementationField.SetValue(null, internalProxy);
    }

    private static object CreateInternalProxy(Type internalInterface, object target)
    {
        var assemblyName = new AssemblyName("DynamicProxyAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        var typeBuilder = moduleBuilder.DefineType("InternalProxy", TypeAttributes.Public | TypeAttributes.Class);

        typeBuilder.AddInterfaceImplementation(internalInterface);

        foreach (var method in internalInterface.GetMethods())
        {
            var parameters = method.GetParameters();
            var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

            var methodBuilder = typeBuilder.DefineMethod(
                method.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                method.ReturnType,
                parameterTypes);

            var ilGenerator = methodBuilder.GetILGenerator();

            // Load the target object (the proxy)
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, typeBuilder.DefineField("_target", target.GetType(), FieldAttributes.Private));

            // Load the arguments
            for (int i = 0; i < parameters.Length; i++)
            {
                ilGenerator.Emit(OpCodes.Ldarg, i + 1);
            }

            // Call the method on the target object
            ilGenerator.Emit(OpCodes.Callvirt, target.GetType().GetMethod(method.Name));
            ilGenerator.Emit(OpCodes.Ret);
        }

        // Define the target field
        typeBuilder.DefineField("_target", target.GetType(), FieldAttributes.Private);

        var proxyType = typeBuilder.CreateType();
        var proxyInstance = Activator.CreateInstance(proxyType, target);

        return proxyInstance;
    }

    public IDisposable Capture()
    {
        StartCapture();
        return Disposable.Create(StopCapture);
    }

    public void StartCapture() => _isInCaptureScope = true;

    public void StopCapture() => _isInCaptureScope = false;
}