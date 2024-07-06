namespace RxSpy.AspNet;

public sealed partial class RxSpyHttpMiddleware : IDisposable
{
    private bool _isDisposed;

    ~RxSpyHttpMiddleware() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            foreach (var subscriber in Subscribers.Values)
            {
                subscriber.Writer.TryComplete();
            }

            Subscribers.Clear();
        }

        _isDisposed = true;
    }
}