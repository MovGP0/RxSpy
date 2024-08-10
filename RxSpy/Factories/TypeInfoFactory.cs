using RxSpy.Entities;

namespace RxSpy.Factories;

public static class TypeInfoFactory
{
    [Pure]
    public static TypeInfo Create(Type type)
    {
        return new()
        {
            Name = type.Name,
            Namespace = type.Namespace
        };
    }

    [Pure]
    public static TypeInfo Create(System.Reflection.TypeInfo info)
    {
        return new()
        {
            Name = info.Name,
            Namespace = info.Namespace
        };
    }
}