using System.Reactive.Disposables;
using System.Reactive.Subjects;
using RxSpy.Events;
using RxSpy.Protobuf.Events;

namespace RxSpy.Observables;

internal sealed class ConnectableOperatorConnection<T> : OperatorConnection<T>, IConnectableObservable<T>
{
    private readonly IConnectableObservable<T> _connectableObservable;

    public ConnectableOperatorConnection(
        RxSpySession session,
        IConnectableObservable<T> parent,
        OperatorInfo childInfo)
        : base(session, parent, childInfo)
    {
        _connectableObservable = parent;
    }

    public IDisposable Connect()
    {
        var connectionId = Session.OnConnected(OperatorInfo);
        var disp = _connectableObservable.Connect();

        return Disposable.Create(() =>
        {
            disp.Dispose();
            Session.OnDisconnected(EventFactory.Disconnect(connectionId));
        });
    }
}