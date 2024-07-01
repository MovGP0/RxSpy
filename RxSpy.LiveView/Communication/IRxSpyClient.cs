using Google.Protobuf;

namespace RxSpy.Communication;

public interface IRxSpyClient
{
    IObservable<IMessage> Connect(Uri address, TimeSpan timeout);
}