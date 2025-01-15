using Microsoft.Extensions.DependencyInjection;
using Proto;

namespace RaceTimings.ProtoActorServer;

public class ActorDependencyResolver(IServiceProvider serviceProvider)
{
    public Props CreateProps<TActor>(params object[] args) where TActor : IActor
    {
        return Props.FromProducer(() =>
        {
            // Resolve the actor type TActor via DI
            var scope = serviceProvider.CreateScope();
            var actor = ActivatorUtilities.CreateInstance<TActor>(scope.ServiceProvider, args);
            return actor;
        });
    }
}