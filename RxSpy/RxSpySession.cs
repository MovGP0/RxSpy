using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using RxSpy.Protobuf.Events;

namespace RxSpy;

public sealed class RxSpySession: IRxSpyEventHandler
{
    private static int _launched = 0;
    private readonly IRxSpyEventHandler _eventHandler;
    private readonly bool _explicitCapture;
    private bool _isInCaptureScope;

    public bool IsCapturing => _explicitCapture == false || _isInCaptureScope == true;

    internal static RxSpySession? Current { get; private set; }

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
            throw new InvalidOperationException("Session already created");

        string pathToGui = FindGuiPath(pathToRxSpy);

        if (pathToGui == null)
            throw new FileNotFoundException("Could not locate RxSpy.LiveView.exe");

        Debug.WriteLine("RxSpy server running at " + address);

        var psi = new ProcessStartInfo(pathToGui)
        {
            Arguments = address.AbsoluteUri
        };

        Process.Start(psi);
    }

    public static RxSpySession Launch(
        IRxSpyEventHandler eventHandler,
        bool explicitCapture = false)
    {
        var session = new RxSpySession(eventHandler, explicitCapture);
        Current = session;

        if (Interlocked.CompareExchange(ref _launched, 1, 0) != 0)
            throw new InvalidOperationException("Session already created");

        InstallInterceptingQueryLanguage(session);

        return session;
    }

    private static string FindGuiPath(string? explicitPathToRxSpy)
    {
        // Try a few different things attempting to find RxSpy.LiveView.exe
        // depending on how things are configured
        if (explicitPathToRxSpy != null) return explicitPathToRxSpy;

        // Same directory as us?
        var ourAssembly = typeof(RxSpySession).Assembly;
        var rxSpyDir = Path.GetDirectoryName(ourAssembly.Location);
        var target = Path.Combine(rxSpyDir, "RxSpy.LiveView.exe");
        if (File.Exists(target))
        {
            return target;
        }

        // Attempt to find the solution directory
        var st = new StackTrace(true);
        var firstExternalFrame = Enumerable.Range(0, 1000)
            .Select(x => st.GetFrame(x))
            .First(x => x.GetMethod().DeclaringType.Assembly != ourAssembly);

        var di = new DirectoryInfo(Path.GetDirectoryName(firstExternalFrame.GetFileName()));

        while (di != null)
        {
            if (di.GetFiles("*.sln").Any())
            {
                break;
            }
            di = di.Parent;
        }

        // Debug mode?
        var fi = new FileInfo(Path.Combine(di.FullName, "RxSpy.LiveView", "bin", "Debug", "RxSpy.LiveView.exe"));
        if (fi.Exists) return fi.FullName;

        // Attempt to track down our own version
        fi = new FileInfo(Path.Combine(di.FullName,
            "packages",
            $"RxSpy.LiveView.{ourAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version}",
            "tools",
            "RxSpy.LiveView.exe"));
        if (fi.Exists) return fi.FullName;

        throw new ArgumentException("Can't find RxSpy.LiveView.exe - either copy it and its DLLs to your output directory or pass in a path to Create");
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

        // Create a proxy for the IQueryLanguage interface
        IInterceptor interceptor = new QueryLanguageInterceptor(session._eventHandler);
        var proxy = proxyGenerator.CreateInterfaceProxyWithTargetInterface(
            iQueryLanguage,
            originalImpl,
            interceptor);

        if (proxy is null)
        {
            return;
        }

        defaultImplementationField.SetValue(null, proxy);
    }

    public static IDisposable Capture()
    {
        if (Current is null)
        {
            return Disposable.Empty;
        }

        Current.StartCapture();
        return Disposable.Create(() => Current.StopCapture());
    }

    public void StartCapture() => _isInCaptureScope = true;

    public void StopCapture() => _isInCaptureScope = false;

    public void OnCreated(OperatorCreatedEvent onCreatedEvent) => _eventHandler.OnCreated(onCreatedEvent);

    public void OnCompleted(OnCompletedEvent onCompletedEvent) => _eventHandler.OnCompleted(onCompletedEvent);

    public void OnError(OnErrorEvent onErrorEvent) => _eventHandler.OnError(onErrorEvent);

    public void OnNext(OnNextEvent onNextEvent) => _eventHandler.OnNext(onNextEvent);

    public void OnSubscribe(SubscribeEvent subscribeEvent) => _eventHandler.OnSubscribe(subscribeEvent);

    public void OnUnsubscribe(UnsubscribeEvent unsubscribeEvent) => _eventHandler.OnUnsubscribe(unsubscribeEvent);

    public void OnConnected(ConnectedEvent connectedEvent) => _eventHandler.OnConnected(connectedEvent);

    public void OnDisconnected(DisconnectedEvent disconnectedEvent) => _eventHandler.OnDisconnected(disconnectedEvent);

    public void OnTag(TagOperatorEvent tagEvent) => _eventHandler.OnTag(tagEvent);

    #region IDisposable
    
    private bool _isDisposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool _)
    {
        if (_isDisposed)
        {
            return;
        }

        _eventHandler.Dispose();
        _isDisposed = true;
    }

    ~RxSpySession() => Dispose(false);

    #endregion
}