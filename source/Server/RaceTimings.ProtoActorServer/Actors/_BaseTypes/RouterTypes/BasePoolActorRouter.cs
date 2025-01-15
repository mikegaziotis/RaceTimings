using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Router;
using RaceTimings.ProtoActorServer.Cache;

namespace RaceTimings.ProtoActorServer;

public abstract class BasePoolActorRouter<TForActor>(
    string routerId,
    ActorDependencyResolver actorDependencyResolver,
    ILogger logger,
    PoolType poolType,
    int poolSize, 
    Func<string,uint>? hash = null, 
    int replicaCount = 100, 
    Func<object,string>? messageHasher = null): AbstractIdActor<string>(routerId, logger) where TForActor: IActor
{
    private PID _pool = null!;
    private readonly Behavior _routerBehavior = null!;
    
    protected override Maybe<string> TryGetIdFromString(string keyPart) => CacheKeyConverter.TryGetStringIdFromString(keyPart);

    protected virtual Task ActiveRouter(IContext context)
    {
        context.Forward(_pool);
        return Task.CompletedTask;
    }
         

    protected virtual Props CreateActorProps() => actorDependencyResolver.CreateProps<TForActor>();
    
    public override Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case Started:
                SpawnNewRouter(context);
                _routerBehavior.Become(ActiveRouter);
                break;
            case Stopping:
                Logger.LogInformation("Router actor stopping");
                break;
            case PoisonPill:
                Logger.LogInformation("Router actor received poison pill");
                break;
        }
        return _routerBehavior.ReceiveAsync(context);
    }

    private void SpawnNewRouter(IContext context)
    {
        var actorProps = CreateActorProps();
        var poolProps = poolType switch
        {
            PoolType.ConsistentHash => context.NewConsistentHashPool(actorProps, poolSize, hash, replicaCount, messageHasher),
            PoolType.RoundRobin => context.NewRoundRobinPool(actorProps, poolSize),
            PoolType.Random => context.NewRandomPool(actorProps, poolSize),
            PoolType.Broadcast => context.NewBroadcastPool(actorProps, poolSize),
            _ => throw new NotImplementedException()            
        };
        _pool = context.SpawnNamed(poolProps, ActorId);
    }
  
}
public enum PoolType
{
    ConsistentHash,
    RoundRobin,
    Random,
    Broadcast,
}

