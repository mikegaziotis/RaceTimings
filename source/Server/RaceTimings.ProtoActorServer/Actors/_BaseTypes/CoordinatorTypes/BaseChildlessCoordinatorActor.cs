using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Proto;
using RaceTimings.Messages;
using RaceTimings.ProtoActorServer.Entities;
using RaceTimings.ProtoActorServer.Repositories;

namespace RaceTimings.ProtoActorServer;

public abstract class BaseChildlessCoordinatorActor<TEntity,TKey>: AbstractCoordinatorActor<TEntity,TKey> 
    where TEntity: class, IEntityWithId<TKey> 
    where TKey : notnull
{

    protected BaseChildlessCoordinatorActor(TKey actorId, ILogger logger, EntityRepository repo) : base(actorId, logger)
    {
        ArgumentNullException.ThrowIfNull(repo);
        _repository = repo;
    }
    
    #region Fields

    private readonly EntityRepository _repository;
    
    #endregion
    

    
    #region Handler Methods
    protected virtual async Task HandleEntityCheckExistsRequest<TResponse>(IContext context, TKey entityId, Func<TResponse> getSuccessResponse, Func<ErrorCode,TResponse> getFailureResponse) where TResponse : IResponse
    {
        var response = (await CheckEntityExistsInStore(entityId)).Match(
            onSuccess: exists => exists ? getSuccessResponse() : getFailureResponse(EntityNotFoundErrorCode),
            onFailure: getFailureResponse);
        context.Respond(response);    
    }
    
    protected virtual async Task HandleEntitiesGetAllRequest<TResponse>(IContext context, Func<IEnumerable<TEntity>,TResponse> getSuccessResponse, Func<ErrorCode,TResponse> getFailureResponse) where TResponse : IResponse
    {
        var response = (await GetEntitiesFromStore()).Match(
            getSuccessResponse,
            getFailureResponse);
        context.Respond(response);
    }
    
    protected virtual async Task HandleEntityGetRequest<TResponse>(IContext context, TKey entityId, Func<TEntity,TResponse> getSuccessResponse, Func<ErrorCode,TResponse> getFailureResponse) where TResponse : IResponse
    {
        var response = (await GetEntityFromStore(entityId)).Match(
            getSuccessResponse,
            getFailureResponse);
        context.Respond(response);
    }
    
    protected virtual async Task HandleEntityAddRequest<TResponse>(IContext context, TEntity newState, Func<TEntity,Maybe<ErrorCode>>? validation,Func<TEntity,TResponse> getSuccessResponse, Func<ErrorCode,TResponse> getFailureResponse) where TResponse : IResponse
    {
        var response = validation is null 
            ? await TrySaveToStore(newState)
            : await validation(newState).Match(
                Some: errorCode => Task.FromResult(getFailureResponse(errorCode)),
                None: async () => await TrySaveToStore(newState));
        context.Respond(response);
        return;

        async Task<TResponse> TrySaveToStore(TEntity entity)
        {
            return (await SaveEntityToStore(entity, true)).Match(
                Some: getFailureResponse,
                None: () => getSuccessResponse(newState));
        }

    }

    protected virtual async Task HandleEntityUpdateRequest<TResponse>(IContext context, TEntity newState, Func<TEntity, TEntity, Maybe<ErrorCode>>? validation, Func<TEntity, TResponse> getSuccessResponse, Func<ErrorCode, TResponse> getFailureResponse) where TResponse : IResponse
    {
        var response = validation is null
            ? await TryUpdateEntity(newState)
            : await (await GetEntityFromStore(newState.Id)).Match(
                async entity => await TryValidateThenUpdateEntity(entity),
                errorCode => Task.FromResult(getFailureResponse(errorCode)));
        context.Respond(response);    
        return;

        async Task<TResponse> TryValidateThenUpdateEntity(TEntity entity)
        {
            return await validation(entity, newState).Match(
                Some: errorCode => Task.FromResult(getFailureResponse(errorCode)),
                None: async () => await TryUpdateEntity(newState));
        }

        async Task<TResponse> TryUpdateEntity(TEntity entity)
        {
            var saveResult = await SaveEntityToStore(entity, true);
            return saveResult.Match(
                Some: getFailureResponse,
                None: () => getSuccessResponse(entity));
        }
    }

    protected virtual async Task HandleArchiveEntityRequest<TResponse>(IContext context, TKey entityId, Func<TResponse> getSuccessResponse, Func<ErrorCode, TResponse> getFailureResponse) where TResponse : IResponse
    {
        var checkExistsResult = await CheckEntityExistsInStore(entityId);
        var response = await checkExistsResult.Match(
            onSuccess: async exists => exists ? await TryRemoveEntity(entityId) : getFailureResponse(EntityNotFoundErrorCode),
            onFailure: errorCode => Task.FromResult(getFailureResponse(errorCode)));
        context.Respond(response);
        return;

        async Task<TResponse> TryRemoveEntity(TKey id)
        {
            var removeResult = await RemoveEntityFromStore(id);
            return removeResult.Match(
                Some: getFailureResponse,
                None: getSuccessResponse
            );
        }
    }
 
    #endregion
    
    #region Store Actions

    private async Task<Result<bool,ErrorCode>> CheckEntityExistsInStore(TKey entityId)
    {
        try
        {
            return await _repository.CheckExistsAsync<TEntity,TKey>(entityId, GetStoreKey);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error checking if {typeof(TEntity).Name} with id {entityId} exists");
            return Result.Failure<bool,ErrorCode>(StoreAccessErrorCode);
        }
        
    }
    
    private async Task<Result<IEnumerable<TEntity>,ErrorCode>> GetEntitiesFromStore()
    {
        try
        {
            var result = await _repository.GetAllAsync<TEntity,TKey>(EntityKeyCollection);
            return Result.Success<IEnumerable<TEntity>,ErrorCode>(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error getting all {typeof(TEntity).Name} rows from store");
            return Result.Failure<IEnumerable<TEntity>,ErrorCode>(StoreAccessErrorCode);
        }
    }
    

   
    private async Task<Result<TEntity,ErrorCode>> GetEntityFromStore(TKey entityId)
    {
        try
        {
            var result = await _repository.GetAsync<TEntity,TKey>(entityId, GetStoreKey, EntityKeyCollection);
            return result.Match(Some: 
                entity => entity, 
                None: () => Result.Failure<TEntity,ErrorCode>(EntityNotFoundErrorCode)); 
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error getting a {typeof(TEntity).Name} from store");
            return Result.Failure<TEntity,ErrorCode>(StoreAccessErrorCode);
        }
    }
    
    private async Task<Maybe<ErrorCode>> SaveEntityToStore(TEntity entity, bool isNewEntity = false)
    {
        return isNewEntity ? await SaveWithDupeCheck(entity) : await SaveEntity(entity);

        async Task<Maybe<ErrorCode>> SaveWithDupeCheck(TEntity newEntity)
        {
            var existsResult = await CheckEntityExistsInStore(entity.Id);
            return await existsResult.Match(
                onSuccess: async exists =>
                    exists ? Maybe<ErrorCode>.From(EntityConflictErrorCode) : await SaveEntity(newEntity),
                onFailure: errorCode => Task.FromResult(Maybe<ErrorCode>.From(errorCode))
            );
        }

        async Task<Maybe<ErrorCode>> SaveEntity(TEntity newEntity)
        {
            try
            {
                await _repository.AddOrUpdateAsync<TEntity,TKey>(newEntity,GetStoreKey, EntityKeyCollection);
                return Maybe<ErrorCode>.None;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error saving {typeof(TEntity).Name} of Id {newEntity.Id} to store");
                return Maybe<ErrorCode>.From(StoreAccessErrorCode);
            }
        }
    }

    private async Task<Maybe<ErrorCode>> RemoveEntityFromStore(TKey entityId)
    {
        return await (await CheckEntityExistsInStore(entityId)).Match(
            onSuccess: async exists => exists ? await TryRemoveFromStore() : Maybe<ErrorCode>.From(EntityNotFoundErrorCode),
            onFailure: errorCode => Task.FromResult(Maybe<ErrorCode>.From(errorCode)));
        
        async Task<Maybe<ErrorCode>> TryRemoveFromStore()
        {
            try
            {
                await _repository.DeleteAsync<TEntity,TKey>(entityId,GetStoreKey,EntityKeyCollection);
                return Maybe<ErrorCode>.None;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error removing {typeof(TEntity).Name} with id {entityId} from store");
                return Maybe<ErrorCode>.From(StoreAccessErrorCode);
            }
        }
    }
    
    #endregion
}