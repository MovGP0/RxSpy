using RxSpy.Events;

namespace RxSpy.Models.Events;

public sealed class TypeInfo: ITypeInfo
{
    public string Name { get; set; }
    public string Namespace { get; set; }
}