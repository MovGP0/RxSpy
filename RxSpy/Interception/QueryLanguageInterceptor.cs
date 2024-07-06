using System.Reactive.Linq;
using Castle.DynamicProxy;
using RxSpy.Constants;
using RxSpy.Events;
using RxSpy.Factories;
using RxSpy.Observables;

namespace RxSpy.Interception;

internal sealed class QueryLanguageInterceptor(RxSpySession session) : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        if (session.IsCapturing)
        {
            invocation.Proceed();
            return;
        }

        try
        {
            switch (invocation.Method.Name)
            {
                case QueryLanguageMethodNames.Create:
                    session.OnCreated(ComposeOperatorCreatedEvent(invocation));
                    break;

                case QueryLanguageMethodNames.Subscribe:
                    session.OnSubscribe(ComposeSubscribeEvent(invocation));
                    break;

                case ObserverMethodNames.OnNext:
                    session.OnNext(ComposeOnNextEvent(invocation));
                    break;

                case ObserverMethodNames.OnCompleted:
                    session.OnCompleted(ComposeOnCompletedEvent(invocation));
                    break;

                case ObserverMethodNames.OnError:
                    session.OnError(ComposeOnErrorEvent(invocation));
                    break;

                case OperatorObservableMethodNames.Tag:
                    session.OnTag(ComposeTagOperatorEvent(invocation));
                    break;
            }
        }
        finally
        {
            // invoke the method on the original implementation
            invocation.Proceed();
        }
    }

    /// <seealso cref="OperatorObservable{T}.Tag(string)"/>
    private static TagOperatorEvent ComposeTagOperatorEvent(IInvocation invocation)
    {
        var methodInfo = MethodInfoFactory.Create(invocation.Method);
        var stackFrame = StackFrameFactory.GetForMethod(invocation.Method);
        var callSite = CallSiteFactory.Create(stackFrame);
        var operatorInfo = OperatorInfoFactory.Create(callSite, methodInfo);

        var tag = invocation.Arguments.Length >= 1 && invocation.Arguments[0] is string tagString
            ? tagString
            : string.Empty;

        return EventFactory.Tag(operatorInfo, tag);
    }

    /// <seealso cref="IObserver{T}.OnError"/>
    private static OnErrorEvent ComposeOnErrorEvent(IInvocation invocation)
    {
        var methodInfo = MethodInfoFactory.Create(invocation.Method);
        var stackFrame = StackFrameFactory.GetForMethod(invocation.Method);
        var callSite = CallSiteFactory.Create(stackFrame);
        var operatorInfo = OperatorInfoFactory.Create(callSite, methodInfo);

        var error = invocation.Arguments.Length >= 1 && invocation.Arguments[0] is Exception exception
            ? exception
            : null;

        return EventFactory.OnError(operatorInfo, error);
    }

    /// <seealso cref="IObserver{T}.OnCompleted"/>
    private static OnCompletedEvent ComposeOnCompletedEvent(IInvocation invocation)
    {
        var methodInfo = MethodInfoFactory.Create(invocation.Method);
        var stackFrame = StackFrameFactory.GetForMethod(invocation.Method);
        var callSite = CallSiteFactory.Create(stackFrame);
        var operatorInfo = OperatorInfoFactory.Create(callSite, methodInfo);
        return EventFactory.OnCompleted(operatorInfo);
    }

    /// <seealso cref="IObserver{T}.OnNext"/>
    private static OnNextEvent ComposeOnNextEvent(IInvocation invocation)
    {
        var methodInfo = MethodInfoFactory.Create(invocation.Method);
        var stackFrame = StackFrameFactory.GetForMethod(invocation.Method);
        var callSite = CallSiteFactory.Create(stackFrame);
        var operatorInfo = OperatorInfoFactory.Create(callSite, methodInfo);

        // get the value of the argument
        object? value = invocation.Arguments[0];

        // get the type of the method
        var valueType = invocation.Method.IsGenericMethod
            ? invocation.Method.GetGenericArguments()[0]
            : value is null
                ? typeof(object)
                : value.GetType();

        return EventFactory.OnNext(operatorInfo, valueType, value);
    }

    /// <summary>
    /// Creates an object of type <see cref="OperatorCreatedEvent"/> based on the method invocation.
    /// </summary>
    /// <param name="invocation">The method invocation.</param>
    /// <returns>An object of type <see cref="OperatorCreatedEvent"/>.</returns>
    /// <seealso cref="IQueryLanguage.Subscribe{TSource}(System.Collections.Generic.IEnumerable{TSource},System.IObserver{TSource})"/>
    private static SubscribeEvent ComposeSubscribeEvent(IInvocation invocation)
    {
        var args = invocation.Arguments;
        var source = args is { Length: >= 1 } ? args[0] : null;
        var observer = args is { Length: >= 2 } ? args[1] : null;

        var methodInfo = MethodInfoFactory.Create(invocation.Method);

        var sourceStackFrame = StackFrameFactory.GetForObject(source);
        var sourceCallSite = CallSiteFactory.Create(sourceStackFrame);
        var sourceOperatorInfo = OperatorInfoFactory.Create(sourceCallSite, methodInfo);

        var observerStackFrame = StackFrameFactory.GetForObject(observer);
        var observerCallSite = CallSiteFactory.Create(observerStackFrame);
        var observerOperatorInfo = OperatorInfoFactory.Create(observerCallSite, methodInfo);
        return EventFactory.Subscribe(observerOperatorInfo, sourceOperatorInfo);
    }

    /// <summary>
    /// Creates an object of type <see cref="OperatorCreatedEvent"/> based on the method invocation.
    /// </summary>
    /// <param name="invocation">The method invocation.</param>
    /// <returns>An object of type <see cref="OperatorCreatedEvent"/>.</returns>
    /// <seealso cref="System.Reactive.Linq.IQueryLanguage.Create{TSource}(Func{IObserver{TSource}, IDisposable})"/>
    private static OperatorCreatedEvent ComposeOperatorCreatedEvent(IInvocation invocation)
    {
        var args = invocation.Arguments;
        var func = args is { Length: >= 1 } ? args[0] : null; // Func<IObserver<TSource>, ...>

        var stackFrame = StackFrameFactory.GetForObject(func);
        var callSite = CallSiteFactory.Create(stackFrame);
        var methodInfo = MethodInfoFactory.Create(invocation.Method);
        var operatorInfo = OperatorInfoFactory.Create(callSite, methodInfo);
        return EventFactory.OperatorCreated(operatorInfo);
    }
}