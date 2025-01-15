using Microsoft.Extensions.Logging;

namespace RaceTimings.ProtoActorServer.Actors;

public class RaceRouterActor(
    string routerId,
    ActorDependencyResolver actorDependencyResolver,
    ILogger logger,
    PoolType poolType,
    int poolSize)
    : BasePoolActorRouter<RaceCoordinatorActor>(routerId, actorDependencyResolver, logger, poolType, poolSize);
