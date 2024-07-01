using System.Reactive.Disposables;
using RxSpy.Events;
using RxSpy.Protobuf.Events;

namespace RxSpy.Observables;

internal class OperatorObservable<T> : IObservable<T>, IOperatorObservable
{
    private readonly IObservable<T> _inner;

    protected RxSpySession Session { get; }
    public OperatorInfo OperatorInfo { get; }

    // We can't subscribe an extra observer to out inner observable just for the event stuff
    // since that could potentially modify the behavior of the inner (like if it was designed 
    // for one subscription only. So instead of doing that we wrap all incoming observers in
    // a private class which forwards all signals while keeping track of whether or not it's
    // the wrapper currently responsible for reporting events.
    private Observer _currentlyReportingObserver;

    private sealed class Observer: IObserver<T>, IDisposable
    {
        private readonly OperatorObservable<T> _parent;
        private readonly IObserver<T> _inner;

        private bool _isReporting;

        public Observer(OperatorObservable<T> parent, IObserver<T> inner)
        {
            _parent = parent;
            _inner = inner;
        }

        private bool isReporting()
        {
            if (!_isReporting && Interlocked.CompareExchange(
                    ref _parent._currentlyReportingObserver,
                    this,
                    null) == null)
            {
                _isReporting = true;
            }

            return _isReporting;
        }

        public void OnCompleted()
        {
            if (isReporting())
                _parent.Session.OnCompleted(EventFactory.OnCompleted(_parent.OperatorInfo));

            _inner.OnCompleted();
        }

        public void OnError(Exception error)
        {
            if (isReporting())
                _parent.Session.OnError(EventFactory.OnError(_parent.OperatorInfo, error));
                
            _inner.OnError(error);
        }

        public void OnNext(T value)
        {
            if (isReporting())
                _parent.Session.OnNext(EventFactory.OnNext(_parent.OperatorInfo, typeof(T), value));

            _inner.OnNext(value);
        }

        public void Dispose() 
        {
            if (_isReporting)
            {
                Interlocked.CompareExchange(ref _parent._currentlyReportingObserver, null, this);
                _isReporting = false;
            }
        }
    }

    public OperatorObservable(RxSpySession session, IObservable<T> inner, OperatorInfo operatorInfo)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        Session = session;
        OperatorInfo = operatorInfo;
        Session.OnCreated(EventFactory.OperatorCreated(operatorInfo));
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        var obs = new Observer(this, observer);
        var disp = _inner.Subscribe(obs);

        return new CompositeDisposable(disp, obs);
    }

    public override string ToString()
    {
        return OperatorInfo.ToString();
    }

    internal void Tag(string tag)
    {
        Session.OnTag(EventFactory.Tag(OperatorInfo, tag));
    }
}