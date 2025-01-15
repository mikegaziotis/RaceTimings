using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Proto;
using RaceTimings.ProtoActorServer.Entities;
using RaceTimings.ProtoActorServer.Repositories;

namespace RaceTimings.ProtoActorServer;

public abstract class BaseEntityActor<TEntity, TKey>(TKey actorId,
    ILogger logger, 
    IEntityRepository repo,
    int? maxIdleTimeMinutes =null) :AbstractStatefulActor<TEntity, TKey>(actorId, logger, maxIdleTimeMinutes) where TEntity : class, IEntityWithId<TKey> where TKey : notnull
{
    protected abstract override Receive GetBehaviorFromState(TEntity actorState);

    protected override async Task RecoverStateAsync()
    {
        try
        {
            var maybe = await repo.GetAsync<TEntity,TKey>(ActorId, GetStoreKey, EnityStoreKeyCollection);
            maybe.Match(
                Some: state =>
                {
                    ActorState = state;
                    Logger.LogInformation("Recovered actor state for {ActorId}", ActorId);
                    ActorBehavior.Become(GetBehaviorFromState(state));
                },
                None: () =>
                {
                    Logger.LogInformation("No actor state found for {ActorId}", ActorId);
                });
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error deserializing actor state");
        }
    }

    protected override async Task PersistStateAsync(TEntity actorState) =>
        await repo.AddOrUpdateAsync<TEntity,TKey>(actorState,GetStoreKey, EnityStoreKeyCollection);
    

    
}