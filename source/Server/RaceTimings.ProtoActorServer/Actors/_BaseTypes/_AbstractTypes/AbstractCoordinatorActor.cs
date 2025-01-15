using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RaceTimings.Messages;
using RaceTimings.ProtoActorServer.Cache;
using RaceTimings.ProtoActorServer.Entities;

namespace RaceTimings.ProtoActorServer;

public abstract class AbstractCoordinatorActor<TEntity, TKey>(TKey actorId, ILogger logger) : AbstractIdActor<TKey>(actorId, logger)
    where TEntity : class, IEntityWithId<TKey>
    where TKey : notnull
{
    #region Static Helper Methods

    protected static string EntityName => CacheKeyConverter.GetEntityName<TEntity>();
    protected static string GetStoreKey(TKey entityId) => CacheKeyConverter.GetEntityStoreKey<TEntity,TKey>(entityId);
    
    protected static string GetPIDStoreKey(TKey entityId) => CacheKeyConverter.GetPIDStoreKey<TEntity,TKey>(entityId);
    protected static string EntityKeyCollection => CacheKeyConverter.GetEntityKeyCollection<TEntity>();
    

    #endregion
    
    #region Abstract Error codes

    protected abstract ErrorCode EntityNotFoundErrorCode { get; }
    protected abstract ErrorCode EntityConflictErrorCode { get; }
    protected abstract ErrorCode StoreAccessErrorCode { get; }
    
    #endregion
}