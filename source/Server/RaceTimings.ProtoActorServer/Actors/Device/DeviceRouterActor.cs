using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace RaceTimings.ProtoActorServer;

public class DeviceRouterActor(
    string routerId,
    ActorDependencyResolver actorDependencyResolver,
    ILogger logger,
    PoolType poolType,
    int poolSize
) : BasePoolActorRouter<DeviceCoordinatorActor>(routerId, actorDependencyResolver, logger, poolType, poolSize);
