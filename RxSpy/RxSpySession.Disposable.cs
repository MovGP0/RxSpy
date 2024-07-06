namespace RxSpy;

public sealed partial class RxSpySession: IDisposable
{
    private bool _isDisposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool _)
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
    }

    ~RxSpySession() => Dispose(false);
}