using Microsoft.Extensions.Logging;

namespace RaceTimings.ProtoActorServer;

public class AthleteRouterActor(
    string routerId,
    ActorDependencyResolver actorDependencyResolver, 
    ILogger<AthleteRouterActor> logger,
    int numberOfRoutes): BasePoolActorRouter<AthleteCoordinatorActor>(routerId, actorDependencyResolver, logger, PoolType.RoundRobin, numberOfRoutes);

