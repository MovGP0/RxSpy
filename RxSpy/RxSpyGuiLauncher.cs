using System.Diagnostics;
using System.Reflection;

namespace RxSpy;

internal static class RxSpyGuiLauncher
{
    public static void LaunchGui(
        Uri address,
        string? pathToRxSpy = null)
    {
        var pathToGui = FindGuiPath(pathToRxSpy);

        if (pathToGui == null || !File.Exists(pathToGui))
        {
            throw new FileNotFoundException(
                "Could not locate RxSpy.LiveView.exe",
                pathToGui);
        }

        var psi = new ProcessStartInfo(pathToGui)
        {
            Arguments = address.AbsoluteUri
        };

        Process.Start(psi);
    }

    private static string? FindGuiPath(string? explicitPathToRxSpy)
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
}