using Microsoft.Extensions.Logging;
using Proto;

namespace RaceTimings.ProtoActorServer.Actors;

public class RootActor(
    ActorDependencyResolver actorDependencyResolver,
    ILogger<RootActor> logger)
    : IActor
{
    
    private PID deviceCoordinatorActor;
    private PID raceCoordinatorActor;
    private PID mqttCoordinatorActor;
    
    public Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case Started:
                ConfigureCoordinators(context);
                logger.LogInformation("Root actor started");
                break;
            case PoisonPill:
                logger.LogInformation("Root actor received poison pill");
                break;
        }
        return Task.CompletedTask;
    }

    private void ConfigureCoordinators(IContext context)
    {
        var dcaProps = actorDependencyResolver.CreateProps<DeviceCoordinatorActor>();
        deviceCoordinatorActor = context.SpawnNamed(dcaProps, "DeviceCoordinatorActor");
        var rcaProps = actorDependencyResolver.CreateProps<RaceCoordinatorActor>();
        raceCoordinatorActor = context.SpawnNamed(rcaProps, "RaceCoordinatorActor");
        var mqttProps = actorDependencyResolver.CreateProps<MqttCoordinatorActor>();
        mqttCoordinatorActor = context.SpawnNamed(mqttProps, "MqttCoordinatorActor");
        logger.LogInformation("Coordinators started");
    }
}