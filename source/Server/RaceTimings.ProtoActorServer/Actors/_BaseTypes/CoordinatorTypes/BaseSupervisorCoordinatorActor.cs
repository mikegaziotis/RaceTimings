using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Proto;
using ProtoBuf;
using RaceTimings.Messages;
using RaceTimings.ProtoActorServer.Cache;
using RaceTimings.ProtoActorServer.Entities;

namespace RaceTimings.ProtoActorServer;

public abstract class BaseSupervisorCoordinatorActor<TEntity,TKey>: AbstractCoordinatorActor<TEntity,TKey> where TEntity: class, IEntityWithId<TKey> where TKey : notnull
{
    protected BaseSupervisorCoordinatorActor(TKey actorId, ActorDependencyResolver actorDependencyResolver, ILogger logger, IHybridCache cache) : base(actorId, logger)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(actorDependencyResolver);
        ActorDependencyResolver = actorDependencyResolver;
        _cache = cache;
    }
    
    #region Fields

    protected readonly ActorDependencyResolver ActorDependencyResolver;

    private readonly IHybridCache _cache;
    
    #endregion
    
    #region Store Actions

    #region ChildActorPID Actions
    
    private async Task<Result<bool,ErrorCode>> CheckChildActorPIDExistsInStore(TKey entityId)
    {
        try
        {
            return await _cache.KeyExistsAsync(GetPIDStoreKey(entityId));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error check if PID of {typeof(TEntity).Name} with id {entityId} exists in store");
            return Result.Failure<bool,ErrorCode>(StoreAccessErrorCode);
        }
    }
    
    private async Task<Result<Maybe<PID>,ErrorCode>> GetChildActorPID(TKey entityId)
    {
        try
        {
            var result = await _cache.GetAsync<ProtoPID>(GetPIDStoreKey(entityId));
            return result.Match(
                Some: pid => Result.Success<Maybe<PID>,ErrorCode>(pid.ToPID()), 
                None: () => Result.Success<Maybe<PID>,ErrorCode>(Maybe<PID>.None));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error getting PID of {typeof(TEntity).Name} with id {entityId} from store");
            return Result.Failure<Maybe<PID>,ErrorCode>(StoreAccessErrorCode);
        }
    }
    
    private async Task<Maybe<ErrorCode>> StoreChildActorPID(TKey entityId, PID pid)
    {
        try
        {
            await _cache.SetAsync(GetPIDStoreKey(entityId), ProtoPID.FromPID(pid));
            return Maybe<ErrorCode>.None;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error getting PID of {typeof(TEntity).Name} with id {entityId} from store");
            return Maybe<ErrorCode>.From(StoreAccessErrorCode);
        }
    }

    private async Task<Maybe<ErrorCode>> RemoveChildActorPIDFromStore(TKey entityId)
    {
        return await (await CheckChildActorPIDExistsInStore(entityId)).Match(
            onSuccess: async exists => exists ? await TryRemoveFromStore(entityId) : Maybe<ErrorCode>.From(EntityNotFoundErrorCode),
            onFailure: errorCode => Task.FromResult(Maybe<ErrorCode>.From(errorCode)));
        
        async Task<Maybe<ErrorCode>> TryRemoveFromStore(TKey id)
        {
            try
            {
                await _cache.RemoveAsync(GetPIDStoreKey(entityId));
                return Maybe<ErrorCode>.None;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error removing PID for {typeof(TEntity).Name} with id {entityId} from store");
                return Maybe<ErrorCode>.From(StoreAccessErrorCode);
            }
        }
    }
    
    #endregion
  
    #endregion
}

[ProtoContract]
public record ProtoPID: IEntityWithId<string> 
{
    [ProtoMember(1)]
    public required string Address { get; init; }
    
    [ProtoMember(2)]
    public required string Id { get; init; }


    public static ProtoPID FromPID(PID pid) => new ProtoPID
    {
        Address = pid.Address,
        Id = pid.Id

    };
  
    public PID ToPID()=> new(Address, Id);
}