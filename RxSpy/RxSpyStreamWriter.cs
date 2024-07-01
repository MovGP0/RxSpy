using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Google.Protobuf;
using RxSpy.Protobuf.Events;

namespace RxSpy;

public sealed class RxSpyStreamWriter : IRxSpyEventHandler
{
    private string _path;
    private Stream _stream;
    private JsonSerializerOptions _serializerStrategy;
    private ConcurrentQueue<IMessage> _queue = new ConcurrentQueue<IMessage>();
    private CancellationTokenSource _cancellationTokenSource;

    public RxSpyStreamWriter(string path)
    {
        _path = path;
        _serializerStrategy = new JsonSerializerOptions();
        _cancellationTokenSource = new CancellationTokenSource();

        Task.Factory.StartNew(() => RunQueue(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
    }

    public RxSpyStreamWriter(Stream stream)
    {
        _stream = stream;
        _serializerStrategy = new JsonSerializerOptions();
        _cancellationTokenSource = new CancellationTokenSource();

        Task.Factory.StartNew(() => RunQueue(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
    }

    private async Task RunQueue(CancellationToken ct)
    {
        using var sw = GetStreamWriter();

        while (!ct.IsCancellationRequested)
        {
            while (!ct.IsCancellationRequested && _queue.TryDequeue(out var ev))
            {
                sw.WriteLine(JsonSerializer.Serialize(ev, _serializerStrategy));
            }

            await Task.Delay(200, ct);
        }
    }

    private TextWriter GetStreamWriter()
    {
        if (_path != null)
            return new StreamWriter(_path, append: false, encoding: Encoding.UTF8);

        return new StreamWriter(_stream, Encoding.UTF8, 1024, leaveOpen: true);
    }

    private void EnqueueEvent(IMessage ev)
    {
        _queue.Enqueue(ev);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        IMessage ev;

        // Wait for up to half a second for the queue to clear
        for (int i = 0; i < 50; i++)
        {
            if (!_queue.TryPeek(out ev))
                break;

            Thread.Sleep(10);
        }

        _cancellationTokenSource.Cancel();
    }

    public void OnCreated(OperatorCreatedEvent onCreatedEvent)
    {
        EnqueueEvent(onCreatedEvent);
    }

    public void OnCompleted(OnCompletedEvent onCompletedEvent)
    {
        EnqueueEvent(onCompletedEvent);
    }

    public void OnError(OnErrorEvent onErrorEvent)
    {
        EnqueueEvent(onErrorEvent);
    }

    public void OnNext(OnNextEvent onNextEvent)
    {
        EnqueueEvent(onNextEvent);
    }

    public void OnSubscribe(SubscribeEvent subscribeEvent)
    {
        EnqueueEvent(subscribeEvent);
    }

    public void OnUnsubscribe(UnsubscribeEvent unsubscribeEvent)
    {
        EnqueueEvent(unsubscribeEvent);
    }

    public void OnConnected(ConnectedEvent connectedEvent)
    {
        EnqueueEvent(connectedEvent);
    }

    public void OnDisconnected(DisconnectedEvent disconnectedEvent)
    {
        EnqueueEvent(disconnectedEvent);
    }

    public void OnTag(TagOperatorEvent tagEvent)
    {
        EnqueueEvent(tagEvent);
    }
}