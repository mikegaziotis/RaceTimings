using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Proto;

namespace RaceTimings.ProtoActorServer;

public abstract class AbstractIdActor<TKey>: IActor
{
    protected AbstractIdActor(TKey actorId, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(actorId);
        ArgumentNullException.ThrowIfNull(logger);
        ActorId = actorId;
        Logger = logger;
    }

    protected readonly TKey ActorId;
    protected readonly ILogger Logger;
    public abstract Task ReceiveAsync(IContext context);

    protected abstract Maybe<TKey> TryGetIdFromString(string keyPart);
    
    protected void HandleInvalidRequest(IContext context)
    {
        Logger.LogError($"DeviceCoordinatorActor received unhandled message: {context.Message}");
        context.Respond(new InvalidCommandForState{ActorState = "Initialized", CommandName = context.Message?.GetType().Name??"Unknown"});
    }
}