namespace RxSpy.Events;

public interface ITypeInfo
{
    string Name { get; }
    string? Namespace { get; }
}