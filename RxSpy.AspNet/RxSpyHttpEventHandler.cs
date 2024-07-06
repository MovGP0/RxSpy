using System.Text.Json;
using Microsoft.Extensions.Options;
using RxSpy.Events;

namespace RxSpy.AspNet;

public sealed class RxSpyHttpEventHandler : IRxSpyEventHandler
{
    private readonly RxSpyOptions _options;

    public RxSpyHttpEventHandler(IOptions<RxSpyOptions> options)
    {
        _options = options.Value;
    }

    public void OnCreated(OperatorCreatedEvent onCreatedEvent)
    {
        var json = JsonSerializer.Serialize(
            onCreatedEvent,
            _options.JsonSerializerOptions);

        RxSpyHttpMiddleware.EnqueueEvent(json);
    }

    public void OnCompleted(OnCompletedEvent onCompletedEvent)
    {
        var json = JsonSerializer.Serialize(
            onCompletedEvent,
            _options.JsonSerializerOptions);

        RxSpyHttpMiddleware.EnqueueEvent(json);
    }

    public void OnError(OnErrorEvent onErrorEvent)
    {
        var json = JsonSerializer.Serialize(
            onErrorEvent,
            _options.JsonSerializerOptions);

        RxSpyHttpMiddleware.EnqueueEvent(json);
    }

    public void OnNext(OnNextEvent onNextEvent)
    {
        var json = JsonSerializer.Serialize(
            onNextEvent,
            _options.JsonSerializerOptions);

        RxSpyHttpMiddleware.EnqueueEvent(json);
    }

    public void OnSubscribe(SubscribeEvent subscribeEvent)
    {
        var json = JsonSerializer.Serialize(
            subscribeEvent,
            _options.JsonSerializerOptions);

        RxSpyHttpMiddleware.EnqueueEvent(json);
    }

    public void OnUnsubscribe(UnsubscribeEvent unsubscribeEvent)
    {
        var json = JsonSerializer.Serialize(
            unsubscribeEvent,
            _options.JsonSerializerOptions);

        RxSpyHttpMiddleware.EnqueueEvent(json);
    }

    public void OnConnected(ConnectedEvent connectedEvent)
    {
        var json = JsonSerializer.Serialize(
            connectedEvent,
            _options.JsonSerializerOptions);

        RxSpyHttpMiddleware.EnqueueEvent(json);
    }

    public void OnDisconnected(DisconnectedEvent disconnectedEvent)
    {
        var json = JsonSerializer.Serialize(
            disconnectedEvent,
            _options.JsonSerializerOptions);

        RxSpyHttpMiddleware.EnqueueEvent(json);
    }

    public void OnTag(TagOperatorEvent tagEvent)
    {
        var json = JsonSerializer.Serialize(
            tagEvent,
            _options.JsonSerializerOptions);

        RxSpyHttpMiddleware.EnqueueEvent(json);
    }
}