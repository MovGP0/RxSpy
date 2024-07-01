using RxSpy.Protobuf.Events;

namespace RxSpy.Events;

public static class TypeInfoFactory
{
    public static TypeInfo Create(Type type)
    {
        return new()
        {
            Name = type.Name,
            Namespace = type.Namespace
        };
    }
}