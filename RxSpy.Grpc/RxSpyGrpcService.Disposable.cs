namespace RxSpy.Grpc;

public sealed partial class RxSpyGrpcService : IDisposable
{
    private bool _isDisposed;

    ~RxSpyGrpcService() => Dispose(false);

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

        _cancellationTokenSource.Cancel();
        _isDisposed = true;
    }
}