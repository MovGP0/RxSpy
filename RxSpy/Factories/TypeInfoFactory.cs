using RxSpy.Entities;

namespace RxSpy.Factories;

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