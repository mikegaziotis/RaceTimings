using System;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Router;
    
namespace RaceTimings.ProtoActorServer.Actors;

public class MqttRouterActor(
    string routerId,
    ActorDependencyResolver actorDependencyResolver,
    ILogger<MqttRouterActor> logger,
    int numberOfRoutes)
    : BasePoolActorRouter<MqttListenerActor>(routerId, actorDependencyResolver, logger, PoolType.RoundRobin, numberOfRoutes);
