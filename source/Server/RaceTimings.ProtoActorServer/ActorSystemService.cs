using RaceTimings.ProtoActorServer.Actors;

namespace RaceTimings.ProtoActorServer;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Proto;

public class ActorSystemService(
    ActorDependencyResolver actorDependencyResolver, 
    ILogger<ActorSystemService> logger) : IHostedService
{
    private ActorSystem _actorSystem = null!;
    private PID _rootActor= null!;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Actor System...");

        // Initialize ActorSystem
        _actorSystem = new ActorSystem();

        // Spawn root actor (for example, your "parent" actor)
        var raceCoordinatorProps = actorDependencyResolver.CreateProps<RaceCoordinatorActor>();
        _rootActor = _actorSystem.Root.Spawn(raceCoordinatorProps);

        logger.LogInformation("Actor System started!");
        
        // Example: Send a message to the root actor
        //_actorSystem.Root.Send(rootActor, new StartMessage("Hello, actors!"));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Actor System...");
        _actorSystem.Root.Stop(_rootActor);
        // Perform cleanup if necessary; this is where you could gracefully stop actors
        //_actorSystem?.Shutdown();
        
        logger.LogInformation("Actor System stopped.");
        return Task.CompletedTask;
    }
}