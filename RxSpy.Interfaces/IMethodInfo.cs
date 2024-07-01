namespace RxSpy.Events;

public interface IMethodInfo
{
    string? DeclaringType { get; }
    string Name { get; }
    string Namespace { get; }
    string Signature { get; }
}