using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
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
        _ = Assembly.Load(new AssemblyName("System.Reactive"));
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

        // Intercept calls to the public proxy
        IInterceptor interceptor = new QueryLanguageInterceptor(session);
        var proxy = proxyGenerator.CreateInterfaceProxyWithoutTarget<IQueryLanguage>(interceptor);

        if (proxy is null)
        {
            return;
        }

        // Use dynamic assembly generation to create a type that implements the internal interface
        var internalProxy = ProxyFactory.CreateProxy(iQueryLanguage, proxy, originalImpl);

        // replace the default implementation with the proxy
        if (internalProxy is not null)
        {
            defaultImplementationField.SetValue(null, internalProxy);
        }
    }

    public IDisposable Capture()
    {
        StartCapture();
        return Disposable.Create(StopCapture);
    }

    public void StartCapture() => _isInCaptureScope = true;

    public void StopCapture() => _isInCaptureScope = false;
}