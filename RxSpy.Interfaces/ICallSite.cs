namespace RxSpy.Events;

public interface ICallSite
{
    string? File { get; }
    int ILOffset { get; }
    int Line { get; }
    IMethodInfo? Method { get; }
}