using Castle.DynamicProxy;

namespace RxSpy;

internal sealed class QueryLanguageInterceptor(IRxSpyEventHandler eventHandler) : IInterceptor
{
    private readonly IRxSpyEventHandler _eventHandler = eventHandler;

    public void Intercept(IInvocation invocation)
    {
        // TODO: implement the interception logic
        // Create events for the event handler based on the intercepted invocation
        throw new NotImplementedException();
    }
}