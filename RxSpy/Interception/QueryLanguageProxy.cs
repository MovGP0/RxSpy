using System.Reflection;

namespace RxSpy.Interception;

public class QueryLanguageProxy : IPublicQueryLanguage
{
    private readonly object _originalImpl;
    private readonly MethodInfo _emptyMethod;

    public QueryLanguageProxy(object originalImpl)
    {
        _originalImpl = originalImpl;
        _emptyMethod = _originalImpl.GetType().GetMethod("Empty");
    }

    public IObservable<T> Empty<T>()
    {
        return (IObservable<T>)_emptyMethod.MakeGenericMethod(typeof(T)).Invoke(_originalImpl, null);
    }
}