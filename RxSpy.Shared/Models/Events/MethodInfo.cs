using RxSpy.Events;

namespace RxSpy.Models.Events;

public sealed class MethodInfo: IMethodInfo
{
    public string DeclaringType { get; set; }
    public string Name { get; set; }
    public string Namespace { get; set; }
    public string Signature { get; set; }
}