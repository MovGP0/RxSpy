using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using RxSpy.Events;
using OneOf;

namespace RxSpy.Streaming;

public sealed partial class RxSpyStreamWriter
{
    private readonly OneOf<Stream, string> _target;
    private readonly JsonSerializerOptions _serializerStrategy;
    private readonly ConcurrentQueue<IRxSpyEvent> _queue = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public RxSpyStreamWriter(string path, JsonSerializerOptions? options = null)
    {
        _target = path;
        _serializerStrategy = options ?? new JsonSerializerOptions();
        Task.Factory.StartNew(() => RunQueue(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
    }

    public RxSpyStreamWriter(Stream stream, JsonSerializerOptions? options = null)
    {
        _target = stream;
        _serializerStrategy = options ?? new JsonSerializerOptions();
        Task.Factory.StartNew(() => RunQueue(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
    }

    private async Task RunQueue(CancellationToken ct)
    {
#if NET8_0_OR_GREATER
        await using var sw = GetStreamWriter();
#else
        using var sw = GetStreamWriter();
#endif

        while (!ct.IsCancellationRequested)
        {
            while (!ct.IsCancellationRequested && _queue.TryDequeue(out var ev))
            {
                await sw.WriteLineAsync(JsonSerializer.Serialize(ev, _serializerStrategy));
            }

            await Task.Delay(200, ct);
        }
    }

    private StreamWriter GetStreamWriter()
    {
        return !_target.TryPickT0(out var stream, out var path)
            ? new StreamWriter(path, append: false, encoding: Encoding.UTF8)
            : new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true);
    }

    private void EnqueueEvent(IRxSpyEvent ev) => _queue.Enqueue(ev);
}