using Google.Protobuf;

namespace RxSpy.Communication;

internal interface IRxSpyServer: IDisposable
{
    Uri Address { get; }
    void WaitForConnection(TimeSpan timeout);
    void EnqueueEvent(IMessage ev);
}