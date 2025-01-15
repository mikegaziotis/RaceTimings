using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Proto;
using RaceTimings.Extensions;
using RaceTimings.Messages;
using RaceTimings.ProtoActorServer.Cache;
using RaceTimings.ProtoActorServer.Entities;
using RaceTimings.ProtoActorServer.Helpers;
using RaceTimings.ProtoActorServer.Repositories;

namespace RaceTimings.ProtoActorServer.Actors;

public class RaceActor(
    Guid actorId,
    ActorDependencyResolver actorDependencyResolver,
    ILogger<RaceActor> logger,
    IEntityRepository repository
    ): BaseEntityActor<RaceEntity, Guid>(actorId, logger, repository)
{
    private ImmutableArray<LaneAthlete> _laneAthletes = ImmutableArray<LaneAthlete>.Empty;
    private ImmutableArray<LaneDevices> _laneDevicesCollection = ImmutableArray<LaneDevices>.Empty;
    private ImmutableDictionary<LaneAthlete,PID> _raceLaneAthleteActors = ImmutableDictionary<LaneAthlete, PID>.Empty;
    private Guid _startingGunDeviceId = Guid.Empty;   
    #region Behaviors

    private async Task Scheduled(IContext context)
    {
        switch (context.Message)
        {
            case Started:
                context.SpawnNamed(actorDependencyResolver.CreateProps<DeviceCoordinatorActor>(), "DeviceCoordinatorActor");
                await RecoverStateAsync();
                ActorBehavior.Become(GetBehaviorFromState(ActorState.Value));
                break;
            case AddNewRaceRequest r:
                await HandleStateChange(context, new RaceEntity
                {
                    Id = ActorId,
                    Name = r.Name,
                    Location = r.Location,
                    StartDateTime = r.StartDateTime,
                    Status = RaceStatus.Scheduled,
                    LaneCount = r.LaneCount
                }, Scheduled);
                Logger.LogInformation("Race {RaceId} created", ActorId);
                break;
            case RaceAddAthlete request:
                var addAthleteResponse = AddAthlete(ActorState.Value.LaneCount, _laneAthletes, request).Match(
                    IRaceAddAthleteResponse (result) =>
                    {
                        _laneAthletes = result;
                        Logger.LogInformation("Athlete {AthleteId} added to race {RaceId}", request.AthleteId, ActorId);
                        return new RaceAddAthleteSuccess();
                    },
                    errorCode =>
                    {
                        logger.LogError("Athlete {AthleteId} could not be added to race {RaceId}", request.AthleteId, ActorId);
                        return new RaceAddAthleteFailure((int)errorCode, ErrorRegistry.Get(errorCode));
                    });
                context.Respond(addAthleteResponse);
                break;
            case RaceRemoveAthleteRequest request:
                var removeAthleteResponse = RemoveAthlete(_laneAthletes, request).Match(
                    IRaceRemoveAthleteResponse (result) =>
                    {
                        _laneAthletes = result;
                        Logger.LogInformation("Athlete {AthleteId} removed from race {RaceId}", request.AthleteId, ActorId);
                        return new RaceRemoveAthleteSuccess();
                    },
                    errorCode =>
                    {
                        logger.LogError("Athlete {AthleteId} could not be removed from race {RaceId}", request.AthleteId, ActorId);
                        return new RaceRemoveAthleteFailure((int)errorCode, ErrorRegistry.Get(errorCode));
                    });
                context.Respond(removeAthleteResponse);
                break;
            case RaceSwapAthleteLanesRequest request:
                var swapAthleteLanesResponse = SwapAthleteLanes(_laneAthletes, request).Match(
                    IRaceSwapAthleteLanesResponse (result) =>
                    {
                        _laneAthletes = result;
                        Logger.LogInformation(
                            $"Athletes in lanes {request.Origin} and {request.Destination} swapped in race {ActorId}");
                        return new RaceSwapAthleteLanesSuccess();
                    },
                    errorCode =>
                    {
                        Logger.LogError(
                            $"Athletes in lanes {request.Origin} and {request.Destination} could not be swapped in race {ActorId}");
                        return new RaceSwapAthleteLanesFailure((int)errorCode, ErrorRegistry.Get(errorCode));
                    });
                context.Respond(swapAthleteLanesResponse);
                break;
            case RaceAddOrUpdateStartingGun request:
                
            case RaceReady:
                CreateRelevantActors(context);
                await HandleStateChange(context, ActorState.Value with {Status = RaceStatus.ReadyToStart}, Ready);
                
                Logger.LogInformation("Race {RaceId} is ready to start", ActorId);
                break;
            default:
                RespondUnprocessedMessage(context, Logger, nameof(Scheduled), context.Message?.GetType().Name);
                break;
        }
    }

    private async Task Ready(IContext context)
    {
        switch (context.Message)
        {
            case RaceStarted:
                await HandleStateChange(context, ActorState.Value with {Status = RaceStatus.Ongoing}, Ongoing);
                break;
            case RaceCanceled:
                await HandleStateChange(context, ActorState.Value with {Status = RaceStatus.Canceled}, Canceled);
                break;
            default:
                RespondUnprocessedMessage(context, Logger, nameof(Ready), context.Message?.GetType().Name);
                break;
        }
    }
    
    private async Task Ongoing(IContext context)
    {
        switch (context.Message)
        {
            case RaceReset:
                await HandleStateChange(context, ActorState.Value with {Status = RaceStatus.Scheduled}, Scheduled);
                break;
            case RaceFinished:
                await HandleStateChange(context, ActorState.Value with {Status = RaceStatus.Finished}, Finished);
                break;
            default:
                RespondUnprocessedMessage(context, Logger, nameof(Ongoing), context.Message?.GetType().Name);
                break;
        }
    }
    
    private async Task Finished(IContext context)
    {
        switch (context.Message)
        {
            case RaceReset:
                await HandleStateChange(context, ActorState.Value with {Status = RaceStatus.Scheduled}, Scheduled);
                break;
            case RaceCanceled:
                await HandleStateChange(context, ActorState.Value with {Status = RaceStatus.Canceled}, Scheduled);
                break;
            default:
                RespondUnprocessedMessage(context, Logger, nameof(Canceled), context.Message?.GetType().Name);
                break;
        }
    }
    
    private async Task Canceled(IContext context)
    {
        switch (context.Message)
        {
            case RaceReset:
                await HandleStateChange(context, ActorState.Value with {Status = RaceStatus.Scheduled}, Scheduled);
                break;
            default:
                RespondUnprocessedMessage(context, Logger, nameof(Canceled), context.Message?.GetType().Name);
                break;
        }
    }


    /// <summary>
    /// Global behavior. Always applied
    /// </summary>
    /// <param name="context"></param>
    public override async Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case Started:
                await RecoverStateAsync();
                break;
            case Stopped:
                await PersistStateAsync(ActorState.Value);
                break;
            case RaceReset:
                await HandleStateChange(context, ActorState.Value with {Status = RaceStatus.Scheduled}, Scheduled);
                break;
            case RaceCanceled:
                await HandleStateChange(context, ActorState.Value with {Status = RaceStatus.Canceled}, Canceled);
                break;
            case GetRaceStatusRequest:
                context.Respond(ActorState);
                break;
        }
        await ActorBehavior.ReceiveAsync(context);
    }

    protected override Maybe<Guid> TryGetIdFromString(string keyPart) => IdConverter.TryGetGuidIdFromString(keyPart);
    

    #endregion
    
    protected override Receive GetBehaviorFromState(RaceEntity state) => state.Status switch
    {
        RaceStatus.Scheduled => Scheduled,
        RaceStatus.ReadyToStart => Ready,
        RaceStatus.Ongoing => Ongoing,
        RaceStatus.Finished => Finished,
        RaceStatus.Canceled => Canceled,
        _ => throw new ArgumentOutOfRangeException()
    };
    
    //Stateful methods
    private void CreateRelevantActors(IContext context)
    {
        // var mqttPublisherProps = actorDependencyResolver.CreateProps<MqttPublisherActor>();
        // var mqttPublisherPID = context.Spawn(mqttPublisherProps);
        for (var i = 1; i <= ActorState.Value.LaneCount; i++)
        {
            Guid actorId = Guid.NewGuid();
            actorDependencyResolver.CreateProps<RaceLaneEventsActor>(actorId);
        }
        
    }

    //Pure functions contain all the validation and update logic
    #region Pure Functions
    
    [Pure]
    private static Result<ImmutableArray<LaneAthlete>, ErrorCode> AddAthlete(short laneCount, ImmutableArray<LaneAthlete> laneAthletes, RaceAddAthlete addAthlete)
    {
        if (addAthlete.LaneNumber.IsNotBetween(1,laneCount))
            return ErrorCode.InvalidLaneNumber;
        if (laneAthletes.Any(x=>x.Lane==addAthlete.LaneNumber))
            return ErrorCode.LaneAlreadyTaken;
        if (laneAthletes.Any(x => x.AthleteId == addAthlete.AthleteId))
            return ErrorCode.AthleteAlreadyExists;

        return laneAthletes.Add(new LaneAthlete(addAthlete.LaneNumber, addAthlete.AthleteId));
    }

    [Pure]
    private static Result<ImmutableArray<LaneAthlete>, ErrorCode> RemoveAthlete(ImmutableArray<LaneAthlete> laneAthletes, RaceRemoveAthleteRequest ra)
    {
        if(laneAthletes.Any(x => x.AthleteId == ra.AthleteId))
            return ErrorCode.AthleteNotInRace;
        return laneAthletes.Remove(laneAthletes.First(x => x.AthleteId == ra.AthleteId));
    }
    
    [Pure]
    private static Result<ImmutableArray<LaneAthlete>,ErrorCode> SwapAthleteLanes(ImmutableArray<LaneAthlete> laneAthletes, RaceSwapAthleteLanesRequest sal)
    {
        if (sal.Origin == sal.Destination)
            return ErrorCode.CannotSwapAthleteToSameLane;
        if(!laneAthletes.Any(x => x.Lane.IsIn(sal.Origin, sal.Destination)))
            return ErrorCode.CannotSwapEmptyLanes;
       
        return laneAthletes.ToArray().Select(x=>SwapLane(x,sal.Origin,sal.Destination)).ToImmutableArray();

        LaneAthlete SwapLane(LaneAthlete laneAthlete, short origin, short destination) =>
            laneAthlete.Lane switch
            {
                var lane when lane==origin => laneAthlete with { Lane = destination },
                var lane when lane==destination => laneAthlete with { Lane = origin },
                _ => laneAthlete
            }; 
    }
    
    
    #endregion
    
}

public readonly record struct LaneAthlete(short Lane, Guid AthleteId);
public readonly record struct LaneDevices
{
    public short Lane { get; init; }
    public Guid ReactionTimeSensorDeviceId { get; init; }
    public Guid DistanceSensorDeviceId { get; init; }
    public Guid FinishLineSensorDeviceId { get; init; }
}


