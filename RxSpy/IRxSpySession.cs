namespace RxSpy;

public interface IRxSpySession
{
    IDisposable Capture();
    bool IsCapturing { get; }
    void StartCapture();
    void StopCapture();
}