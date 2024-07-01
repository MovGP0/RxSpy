using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using Google.Protobuf;
using ReactiveUI;

namespace RxSpy.Communication;

public class RxSpyHttpClient : ReactiveObject, IRxSpyClient
{
    public IObservable<IMessage> Connect(Uri address, TimeSpan timeout)
    {
        return GetStream(new Uri(address, "stream"), timeout);
    }

    private static IObservable<IMessage> GetStream(Uri address, TimeSpan timeout)
    {
        var client = new HttpClient();
        client.Timeout = timeout;

        var req = new HttpRequestMessage(HttpMethod.Get, address);
        var disp = new CompositeDisposable(req, client);

        var completionOptions = HttpCompletionOption.ResponseHeadersRead;

        return Observable.FromAsync<HttpResponseMessage>(ct => client.SendAsync(req, completionOptions, ct))
            .SelectMany(resp =>
                resp.StatusCode == HttpStatusCode.OK
                    ? Observable.FromAsync(() => resp.Content.ReadAsStreamAsync())
                    : Observable.Throw<Stream>(new Exception("Could not open room stream: " + resp.ReasonPhrase)))
            .SelectMany(ReadEvents)
            .Finally(disp.Dispose);
    }

    private static IObservable<IMessage> ReadEvents(Stream stream)
    {
        return Observable.Create<IMessage>(async (observer, ct) =>
        {
            using var sr = new StreamReader(stream);

            while (await sr.ReadLineAsync(ct) is { } line)
            {
                ct.ThrowIfCancellationRequested();
                var message = JsonSerializer.Deserialize<IMessage>(line);
                if (message is not null)
                {
                    observer.OnNext(message);
                }
            }

            observer.OnCompleted();
        });
    }
}