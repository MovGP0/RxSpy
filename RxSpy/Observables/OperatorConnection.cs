using System.Reactive.Disposables;
using RxSpy.Entities;
using RxSpy.Extensions;

namespace RxSpy.Observables;

internal class OperatorConnection<T> : IObservable<T>, IConnection
{
    private readonly IObservable<T> _parent;
    private readonly OperatorInfo? _parentInfo;

    public OperatorInfo OperatorInfo { get; }

    protected RxSpySession Session { get; }

    public OperatorConnection(
        RxSpySession session,
        IObservable<T> parent,
        OperatorInfo childInfo)
    {
        Session = session;
        _parent = parent;

        if (parent is IOperatorObservable operatorObservable)
            _parentInfo = operatorObservable.OperatorInfo;

        OperatorInfo = childInfo;
    }

    public override string ToString()
    {
        return OperatorInfo + "::Connection";
    }

    public virtual IDisposable Subscribe(IObserver<T> observer)
    {
        // Parent is not a tracked observable.
        if (_parentInfo == null)
        {
            return _parent.Subscribe(observer);
        }

        var subscriptionId = Session.OnSubscribe(OperatorInfo, _parentInfo.Value);

        var disp = _parent.Subscribe(observer);

        return Disposable.Create(() =>
        {
            disp.Dispose();
            Session.OnUnsubscribe(subscriptionId);
        });
    }
}