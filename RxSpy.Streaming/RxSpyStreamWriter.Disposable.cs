namespace RxSpy.Streaming;

public sealed partial class RxSpyStreamWriter : IDisposable
{
    private bool _isDisposed;

    ~RxSpyStreamWriter() => Dispose(false);
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        // Wait for up to half a second for the queue to clear
        for (int i = 0; i < 50; i++)
        {
            if (!_queue.TryPeek(out var ev))
                break;

            Thread.Sleep(10);
        }

        _cancellationTokenSource.Cancel();

        _isDisposed = true;
    }
}