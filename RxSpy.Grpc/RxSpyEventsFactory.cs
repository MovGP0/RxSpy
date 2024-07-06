using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Mapster;
using RxSpy.Protobuf.Events;

namespace RxSpy.Grpc;

internal static class RxSpyEventsFactory
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static RxSpyEvents ToRxSpyEvents(this RxSpy.Events.ConnectedEvent connectedEvent)
        => new() { Connected = connectedEvent.Adapt<ConnectedEvent>() };

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static RxSpyEvents ToRxSpyEvents(this RxSpy.Events.DisconnectedEvent disconnectedEvent)
        => new() { Disconnected = disconnectedEvent.Adapt<DisconnectedEvent>() };

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static RxSpyEvents ToRxSpyEvents(this RxSpy.Events.SubscribeEvent subscribeEvent)
        => new() { Subscribe = subscribeEvent.Adapt<SubscribeEvent>() };

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static RxSpyEvents ToRxSpyEvents(this RxSpy.Events.UnsubscribeEvent unsubscribeEvent)
        => new() { Unsubscribe = unsubscribeEvent.Adapt<UnsubscribeEvent>() };

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static RxSpyEvents ToRxSpyEvents(this RxSpy.Events.OnNextEvent onNextEvent)
        => new() { OnNext = onNextEvent.Adapt<OnNextEvent>() };
    
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static RxSpyEvents ToRxSpyEvents(this RxSpy.Events.OnErrorEvent onErrorEvent)
        => new() { OnError = onErrorEvent.Adapt<OnErrorEvent>() };
    
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static RxSpyEvents ToRxSpyEvents(this RxSpy.Events.OnCompletedEvent onCompletedEvent)
        => new() { OnCompleted = onCompletedEvent.Adapt<OnCompletedEvent>() };
    
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static RxSpyEvents ToRxSpyEvents(this RxSpy.Events.OperatorCreatedEvent operatorCreatedEvent)
        => new() { OperatorCreated = operatorCreatedEvent.Adapt<OperatorCreatedEvent>() };

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static RxSpyEvents ToRxSpyEvents(this RxSpy.Events.TagOperatorEvent tagOperatorEvent)
        => new() { TagOperator = tagOperatorEvent.Adapt<TagOperatorEvent>() };
}