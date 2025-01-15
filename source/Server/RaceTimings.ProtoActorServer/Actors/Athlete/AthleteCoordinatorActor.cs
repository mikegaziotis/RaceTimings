using System.Collections.Immutable;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Proto;
using ProtoBuf;
using RaceTimings.Messages;
using RaceTimings.ProtoActorServer.Entities;
using StackExchange.Redis;

namespace RaceTimings.ProtoActorServer;

public class AthleteCoordinatorActor(
    ActorDependencyResolver actorDependencyResolver, 
    ILogger<AthleteCoordinatorActor> logger, 
    IConnectionMultiplexer redisConnection): IActor
{
    private const string AthleteKeyPattern = "athlete:*";
    private static string GetKeyFromAthleteId(Guid athleteId) => $"athlete:{athleteId}";
    private static Maybe<Guid> GetAthleteIdFromKey(string key) => Guid.TryParse(key.Split(':').Last(), out var guid) ? Maybe.From(guid) : Maybe<Guid>.None;
    
    private ImmutableDictionary<Guid,PID> _athleteActors =  ImmutableDictionary<Guid, PID>.Empty;
    
    public async Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case Started when context.Sender is not null:
                logger.LogInformation($"{nameof(AthleteCoordinatorActor)} has received Started message from Child with PID {context.Sender.Id}");
                break;
            case GetAllAthletesRequest:
                await HandleGetAllAthletesRequest(context);
                break;
            case GetAthleteRequest r:
                await HandleGetAthleteRequest(context, r);
                break;
            case AddNewAthleteRequest:
                await HandleAddNewAthleteRequest(context);
                break;
            case Terminated t:
                await HandleChildTermination(t);
                break;
            case Failure f:
                await HandleChildFailure(f);
                break;
        }
    }

    private Task HandleChildFailure(Failure failure)
    {
        logger.LogError(failure.Reason,$"{nameof(AthleteCoordinatorActor)} received Failure from Actor with id '{failure.Who.Id}' the failure message: {failure.Reason.Message}");
        return Task.CompletedTask;
    }

    private Task HandleChildTermination(Terminated terminated)
    {
        if (Guid.TryParse(terminated.Who.Id, out var id))
        {
            if (_athleteActors.ContainsKey(id))
            {
                _athleteActors = _athleteActors.Remove(id);
            }
            else
            {
                logger.LogError($"{nameof(AthleteCoordinatorActor)} received Terminated message from Actor with id '{id}', but not such id was not found in memory");
            }
        }
        else
        {
            logger.LogError($"{nameof(AthleteCoordinatorActor)} received Terminated message from Actor with id '{terminated.Who.Id}', which wasn't a Guid as expected");
        }
        return Task.CompletedTask;
    }

    private Task HandleAddNewAthleteRequest(IContext context)
    {
        var newId = Guid.CreateVersion7();
        actorDependencyResolver.CreateProps<AthleteActor>(newId)
            .WithChildSupervisorStrategy(new OneForOneStrategy( GetSupervisionDirective, 3, TimeSpan.FromSeconds(1)));
        var pid = context.Spawn(actorDependencyResolver.CreateProps<AthleteActor>(newId));
        context.Forward(pid);
        _athleteActors = _athleteActors.Add(newId, pid);
        return Task.CompletedTask;
    }

    private static SupervisorDirective GetSupervisionDirective(PID pid, Exception reason) => SupervisorDirective.Restart;
    

    private async Task HandleGetAllAthletesRequest(IContext context)
    {
        var athletes = await GetAthletesFromStore(redisConnection, logger);
        context.Respond(new GetAllAthletesResponse{ Athletes = athletes});
    }

    private async Task HandleGetAthleteRequest(IContext context, GetAthleteRequest request)
    {
        if (!_athleteActors.TryGetValue(request.AthleteId, out var pid))
        {
            var device = await GetAthleteFromStore(redisConnection.GetDatabase(), request.AthleteId);
            device.Match(Some: x =>
                {
                    var props = actorDependencyResolver.CreateProps<AthleteActor>(request.AthleteId, actorDependencyResolver, logger);
                    var spawnPid = context.Spawn(props);
                    context.Respond(new GetAthleteResponse(x));
                    _athleteActors = _athleteActors.Add(request.AthleteId, spawnPid);
                }, 
                None: ()=>
                {
                    context.Respond(new GetAthleteResponse(null));
                });
            return;
        }
        var response = await context.RequestAsync<GetAthleteResponse>(pid, new GetAthleteRequest(request.AthleteId));
        context.Respond(response);
    }

    private static async Task<Athlete[]> GetAthletesFromStore(IConnectionMultiplexer multiplexer, ILogger logger)
    {
        var idArray = await GetAthleteIdsFromStore(multiplexer, logger);
        List<Athlete> athletes = [];
        var tasks = idArray.Select(athleteId => GetAthleteFromStore(multiplexer.GetDatabase(), athleteId)).ToArray();
        await Task.WhenAll(tasks);
        tasks.Select(x => x.Result)
            .ToList()
            .ForEach(x => x.Match(
                Some: entity => athletes.Add(entity), 
                None: ()=>{}));
        return athletes.ToArray();
    }
    
    private static async Task<Guid[]> GetAthleteIdsFromStore(IConnectionMultiplexer multiplexer, ILogger logger)
    {
        var server = multiplexer.GetServer(SystemConfiguration.Redis.ConnectionString);
        List<Guid> athleteIds = [];
        await foreach (var redisKey in server.KeysAsync(pattern: AthleteKeyPattern))
        {
            var key = redisKey.ToString();
            GetAthleteIdFromKey(key).Match(athleteId=>athleteIds.Add(athleteId),
                () =>
                {
                    logger.LogError($"Invalid athlete key: {key}");
                });
        }
        return athleteIds.ToArray();
    }
   
    private static async Task<Maybe<Athlete>> GetAthleteFromStore(IDatabase redisDatabase, Guid athleteId)
    {
        var redisValue = await redisDatabase.StringGetAsync(GetKeyFromAthleteId(athleteId));
        if (redisValue.IsNull || !redisValue.HasValue) 
            return Maybe<Athlete>.None;
        using var memoryStream = new MemoryStream(redisValue!);
        var entity = Serializer.Deserialize<AthleteEntity>(memoryStream);
        return Maybe.From(entity.ToAthlete());
    }
}

[ProtoContract]
public record GetAllAthletesRequest;

[ProtoContract]
public record GetAllAthletesResponse
{
    [ProtoMember(1)]
    public Athlete[] Athletes { get; init; } = [];
};

[ProtoContract]
public record GetAthleteRequest([property:ProtoMember(1)]Guid AthleteId);

[ProtoContract]
public record GetAthleteResponse([property:ProtoMember(1)]Athlete? AthleteId);

