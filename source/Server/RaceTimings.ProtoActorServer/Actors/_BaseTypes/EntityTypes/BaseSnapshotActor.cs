using Microsoft.Extensions.Logging;
using Proto;
using Proto.Persistence;
using RaceTimings.Messages;
using RaceTimings.ProtoActorServer.Entities;


namespace RaceTimings.ProtoActorServer;

public abstract class BaseSnapshotActor<TEntity,TKey>: AbstractStatefulActor<TEntity,TKey> where TEntity: class, IEntityWithId<TKey> where TKey : notnull
{

    protected readonly Persistence Persistence;
   
    protected BaseSnapshotActor(ILogger logger, ISnapshotStore snapshotStore, TKey actorId): base(actorId, logger)
    {
        ArgumentNullException.ThrowIfNull(actorId);
        ArgumentNullException.ThrowIfNull(snapshotStore);
        Persistence = Persistence.WithSnapshotting(snapshotStore, actorId.ToString()!, ApplySnapshotHandler);
    }
    
    
    
    private void ApplySnapshotHandler(Snapshot snapshot)
    {
        if (snapshot.State is TEntity actorState && actorState.Id!.Equals(ActorId))
        {
            ActorState = actorState;
        }
        else
        {
            throw new ArgumentException("Invalid snapshot or race ID mismatch");
        }
    }

    public abstract override Task ReceiveAsync(IContext context);

    protected override async Task PersistStateAsync(TEntity actorState)
    {
        await Persistence.PersistSnapshotAsync(actorState);
    }
    
    
}