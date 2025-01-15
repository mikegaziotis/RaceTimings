using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Persistence;
using ProtoBuf;
using RaceTimings.Messages;
using RaceTimings.ProtoActorServer.Entities;


namespace RaceTimings.ProtoActorServer.Actors;

public class RaceLaneEventsActor(ILogger<RaceLaneEventsActor> logger,
    Guid actorId,
    IEventStore eventStore,
    ISnapshotStore snapshotStore,
    PID resultsAggregatorActor,
    PID mqttPublisherActor): BaseSnapshotEventSourceActor<RaceEvents,Guid>(logger, eventStore, snapshotStore, actorId)
{
    private int _distanceIndex;

    protected override async Task Uninitialized(IContext context)
    {
        switch (context.Message)
        {
            case InitializeRaceLaneEvents init:
                await HandleStateChange(context, newState: ActorState.Value with
                {
                    RaceId = init.RaceId,
                    AthleteId = init.AthleteId,
                    Lane = init.Lane,
                    State = RaceLaneEventsState.Initialized,
                    DistanceTimes = [..Enumerable.Repeat(new DistanceTime(0,0), init.Splits)]
                }, newBehavior:Initialized, persist:false);
                break;
            default:
                RespondUnprocessedMessage(context, Logger, nameof(Uninitialized), context.Message!.GetType().Name);
                break;
        }
    }

    private async Task Initialized(IContext context)
    {
        switch (context.Message)
        {
            case GunFired:
                await HandleStateChange(context, newState: ActorState.Value with
                {
                    State = RaceLaneEventsState.Listening
                }, newBehavior:Listening, persist:false);
                break;
            case RaceReset:
                await HandleStateChange(context, newState: new RaceEvents
                {
                    Id = ActorId,
                    State = RaceLaneEventsState.Uninitialized,
                    DistanceTimes = []
                }, newBehavior: Uninitialized, persist: false);
                break;
            default:
                RespondUnprocessedMessage(context, Logger, nameof(Initialized), context.Message!.GetType().Name);
                break;
        }
    }
    
    private async Task Listening(IContext context)
    {
        switch (context.Message)
        {
            case ReactionTime rt:
                await HandleStateChange(context, newState: ActorState.Value with
                {
                    ReactionTimeMs = rt.ReactionTimeMs
                }, persist:false);
                break;
            case DistanceTime dt:
                var currentSpeed = _distanceIndex >= 1 ?
                                    CalculateCurrentSpeedInKmH(dt, ActorState.Value.DistanceTimes[_distanceIndex-1])
                                    :0d;
                await HandleStateChange(context, newState: ActorState.Value with
                {
                    DistanceTimes = ActorState.Value.DistanceTimes.SetItem(_distanceIndex++,dt),
                    MaxSpeed = Math.Max(ActorState.Value.MaxSpeed, currentSpeed),
                });
                context.Send(mqttPublisherActor, new RaceLaneUpdate
                {
                    Lane = ActorState.Value.Lane,
                    DistanceTime = dt,
                    CurrentSpeed = currentSpeed
                });
                break;
            case FinishTime ft:
                await HandleStateChange(context, newState: ActorState.Value with
                {
                    FinishTimeMs = ft.TimeMs,
                    AverageSpeed = ActorState.Value.DistanceTimes.Average(x => (double)x.DistanceCm / x.TimeMs * 36d),
                    State = RaceLaneEventsState.Finished
                }, newBehavior:Finished, persist:false);
                var raceLaneFinished = new RaceLaneFinished
                {
                    Lane = ActorState.Value.Lane,
                    FinishTimeMs = ActorState.Value.FinishTimeMs
                };
                context.Send(mqttPublisherActor, raceLaneFinished);
                context.Send(resultsAggregatorActor, raceLaneFinished);
                break;
            case RaceReset:
                await HandleStateChange(context, newState: new RaceEvents
                {
                    Id = ActorId,
                    State = RaceLaneEventsState.Uninitialized,
                    DistanceTimes = []
                }, newBehavior: Uninitialized, persist: false);
                break;
            default:
                RespondUnprocessedMessage(context, Logger, nameof(Listening), context.Message!.GetType().Name);
                break;
        }
    }

    private async Task Finished(IContext context)
    {
        switch (context.Message)
        {
            case RaceReset:
                await HandleStateChange(context, newState: new RaceEvents
                {
                    Id = ActorId,
                    State = RaceLaneEventsState.Uninitialized,
                    DistanceTimes = []
                }, newBehavior: Uninitialized, persist: false);
                break;
            default:
                RespondUnprocessedMessage(context, Logger, nameof(Finished), context.Message!.GetType().Name);
                break;
        }
    }
    
    
    public override async Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case Started:
                await Persistence.RecoverStateAsync();
                break;
            case Stopping:
                await Persistence.PersistSnapshotAsync(ActorState);
                break;                
        }
        await ActorBehavior.ReceiveAsync(context);
    }

    protected override Maybe<Guid> TryGetIdFromString(string keyPart)
    {
        throw new NotImplementedException();
    }

    protected override Task RecoverStateAsync()
    {
        throw new NotImplementedException();
    }

    protected override Receive GetBehaviorFromState(RaceEvents actorState)
        => actorState.State switch
        {
            RaceLaneEventsState.Uninitialized => Uninitialized,
            RaceLaneEventsState.Initialized => Initialized,
            RaceLaneEventsState.Listening => Listening,
            RaceLaneEventsState.Finished => Finished,
            _ => throw new ArgumentOutOfRangeException()
        };

    protected override void ApplyEventHandler(Event @event)
    {
        throw new NotImplementedException();
    }

    [Pure]
    private static double CalculateCurrentSpeedInKmH(DistanceTime mostRecentMeasurement, DistanceTime previousMeasurement)
    {
        ArgumentNullException.ThrowIfNull(mostRecentMeasurement);
        ArgumentNullException.ThrowIfNull(previousMeasurement);
        
        if (mostRecentMeasurement.DistanceCm == 0 
            || previousMeasurement.DistanceCm == 0 
            || mostRecentMeasurement.TimeMs == 0 
            || previousMeasurement.TimeMs == 0
            || mostRecentMeasurement.DistanceCm == previousMeasurement.DistanceCm 
            || mostRecentMeasurement.TimeMs == previousMeasurement.TimeMs)
            return 0;

        return (double)(mostRecentMeasurement.DistanceCm - previousMeasurement.DistanceCm) / (mostRecentMeasurement.TimeMs - previousMeasurement.TimeMs) * 36;
    }
    
}

[ProtoContract]
public record InitializeRaceLaneEvents
{
    [ProtoMember(1)]
    public Guid RaceId { get; init; }
    [ProtoMember(2)]
    public short Lane { get; init; }
    [ProtoMember(3)]
    public Guid AthleteId { get; init; }
    
    [ProtoMember(4)]
    public int Splits { get; init; }
};

public record RaceEvents: IEntityWithId<Guid>
{
    public required Guid Id { get; init; }
    public Guid RaceId { get; init; }
    public short Lane { get; init; }
    public Guid AthleteId { get; init; }
    public RaceLaneEventsState State { get; init; }
    public int? ReactionTimeMs { get; init; }
    public required ImmutableArray<DistanceTime> DistanceTimes { get; init; } = [];
    public double MaxSpeed { get; init; }
    public double AverageSpeed { get; init; }
    public int FinishTimeMs { get; init; }
};

[ProtoContract]
public record ReactionTime([property:ProtoMember(1)]int ReactionTimeMs);

[ProtoContract]
public record DistanceTime([property:ProtoMember(1)]int DistanceCm, [property:ProtoMember(2)]int TimeMs);

[ProtoContract]
public record FinishTime([property:ProtoMember(1)]int TimeMs);

[ProtoContract]
public record GunFired;

[ProtoContract]
public record RaceLaneFinished
{
    [ProtoMember(1)]
    public short Lane { get; init; }
    [ProtoMember(2)]
    public int FinishTimeMs { get; init; }
}

[ProtoContract]
public record RaceLaneUpdate
{
    
    [ProtoMember(1)]
    public short Lane { get; init; }
    [ProtoMember(2)]
    public required DistanceTime DistanceTime { get; init; }
    [ProtoMember(3)]
    public double CurrentSpeed { get; init; }
}

public enum RaceLaneEventsState
{
    Uninitialized,
    Initialized,
    Listening,
    Finished
}



