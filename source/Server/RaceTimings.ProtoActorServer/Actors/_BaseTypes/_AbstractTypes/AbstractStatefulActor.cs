using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Mailbox;
using ProtoBuf;
using RaceTimings.Messages;
using RaceTimings.ProtoActorServer.Cache;
using RaceTimings.ProtoActorServer.Entities;

namespace RaceTimings.ProtoActorServer;

public abstract class AbstractStatefulActor<TEntity, TKey>: AbstractIdActor<TKey> where TEntity: class, IEntityWithId<TKey> where TKey : notnull
{
    protected Maybe<TEntity> ActorState = Maybe<TEntity>.None;
    protected readonly Behavior ActorBehavior = new();
    private readonly Maybe<int> _maxIdleTimeMinutes;

    protected AbstractStatefulActor(TKey actorId,ILogger logger, int? maxIdleTimeMinutes = null): base(actorId,logger)
    {
        ActorBehavior.Become(Uninitialized);
        _maxIdleTimeMinutes = maxIdleTimeMinutes.AsMaybe();
    }
    
    #region Static Helper Methods
    
    protected static string EntityName => CacheKeyConverter.GetEntityName<TEntity>();
    protected static string ActorTypeName => typeof(TEntity).Name;
    protected static string GetStoreKey(TKey entityId) => CacheKeyConverter.GetEntityStoreKey<TEntity,TKey>(entityId);

    protected static string EnityStoreKeyCollection => CacheKeyConverter.GetEntityKeyCollection<TEntity>();

    #endregion

    #region Behaviors

    protected virtual Task Uninitialized(IContext context)
    {
        Logger.LogError($"Uninitialized actor of type \'{ActorTypeName}\' and ActorId \'{ActorId}\'");
        context.Respond(new UninitializedEntityActorError($"Uninitialized actor of type \'{ActorTypeName}\' and ActorId \'{ActorId}\'"));
        return Task.CompletedTask;
    }
    internal async Task HandleUncertainStateChange<TSuccess, TFailure>(
        IContext context,
        Func<Result<TEntity, ErrorCode>> operation,
        Func<TSuccess> successResponse,
        Func<ErrorCode, TFailure> failureResponse,
        Receive? newBehavior = null) where TFailure : IResponse where TSuccess : IResponse
    {
        await operation().Match(
            async newState =>
            {
                
                ActorState = newState;
                try
                {
                    await PersistStateAsync(ActorState.Value);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error persisting state");
                    throw;
                }
                context.Respond(successResponse());
            },
            error =>
            {
                context.Respond(failureResponse(error));
                return Task.CompletedTask;
            });
        if (newBehavior is not null)
        {
            ActorBehavior.Become(newBehavior);
        }
    }
    
    internal async Task HandleUncertainStateChange(
        IContext context,
        Func<Result<TEntity, ErrorCode>> operation,
        Receive? newBehavior = null)
    {
        await operation().Match(
            async newState =>
            {
                ActorState = newState;
                try
                {
                    await PersistStateAsync(ActorState.Value);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error persisting state");
                    throw;
                }
            },
            _ => Task.CompletedTask);
        if (newBehavior is not null)
        {
            ActorBehavior.Become(newBehavior);
        }
    }

    internal async Task HandleStateChange(IContext context, TEntity newState, Receive? newBehavior = null, bool persist = true)
    {
        ActorState = newState;
        switch (persist)
        {
            case true when ActorState.HasValue:
                try
                {
                    await PersistStateAsync(ActorState.Value);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex,
                        $"Error persisting for Actor of type \'{ActorTypeName}\' and ActorId \'{ActorId}\'");
                    throw;
                }
                break;
            case false:
                Logger.LogError($"Cannot persist an uninitialized entity for Actor of type \'{ActorTypeName}\' and ActorId \'{ActorId}\'");
                break;
        }

        if (newBehavior is not null)
        {
            ActorBehavior.Become(newBehavior);
        }
    }
    
    internal async Task HandleStateChange<TResponse>(IContext context, TEntity newState, TResponse? response, Receive? newBehavior = null) where TResponse : IResponse
    {
        await HandleStateChange(context, newState, newBehavior);
        if (response is not null)
        {
            context.Respond(response);            
        }
    }
    
    #endregion
    
    //Main method message receiving method
    public override async Task ReceiveAsync(IContext context)
    {
        
        switch (context.Message)
        {
            case Started:
                LogStarted(Logger, ActorId.ToString()!);
                await RecoverStateAsync();
                _maxIdleTimeMinutes.Match(minutes => context.SetReceiveTimeout(TimeSpan.FromMinutes(minutes)), () => { });
                break;
            case ReceiveTimeout:
                LogReceivedTimeout(Logger, ActorId.ToString()!);
                await HandleReceivedTimeout(context);
                break;
            case PoisonPill:
                LogPoisonPill(Logger, ActorId.ToString()!);
                await HandlePoisonPill(context);
                break;
            case Restart r:
                LogRestart(Logger, ActorId.ToString()!, r.Reason);
                await HandleRestart(context, r.Reason);
                break;
            case Restarting:
                LogRestarting(Logger, ActorId.ToString()!);
                await HandleRestarting(context);
                break;
            case Stopping:
                LogStopping(Logger, ActorId.ToString()!);
                await HandleStopping(context);
                break;
            case Stopped:
                await HandleStopped(context);
                LogStopped(Logger, ActorId.ToString()!);
                break;
            case Failure f:
                LogFailure(Logger, ActorId.ToString()!, f.Who, f.Reason, f.Message);
                await HandleFailure(context, f);
                break;
            case Terminated t:
                LogChildTerminated(Logger, ActorId.ToString()!, t.Who, t.Why);
                await HandleTerminated(context);
                break;
            case SuspendMailbox:
                LogSuspendedMailbox(Logger, ActorId.ToString()!);
                await HandleSuspendMailbox(context);
                break;
            case ResumeMailbox:
                LogResumedMailbox(Logger, ActorId.ToString()!);
                await HandleResumeMailbox(context);
                break;
            case Watch w:
                LogWatch(Logger, ActorId.ToString()!, w.Watcher.Id);
                await HandleWatch(context);
                break;
            case Unwatch u:
                LogUnwatch(Logger, ActorId.ToString()!, u.Watcher.Id);
                await HandleUnwatch(context);
                break;
            case Touch:
                LogTouch(Logger, ActorId.ToString()!);
                await HandleTouch(context);
                break;

        }
        await ActorBehavior.ReceiveAsync(context);
    }

    #region System Event Handlers
    
    protected virtual async Task HandleReceivedTimeout(IContext context) => await context.StopAsync(context.Self);
    protected virtual Task HandleRestart(IContext context, Exception exception) => Task.CompletedTask;
    protected virtual Task HandleRestarting(IContext context) => Task.CompletedTask;
    protected virtual Task HandleTerminated(IContext context) => Task.CompletedTask;
    protected virtual Task HandleStopped(IContext context) => Task.CompletedTask;
    protected virtual Task HandleStopping(IContext context) => Task.CompletedTask;
    protected virtual Task HandleFailure(IContext context, Failure f) => Task.CompletedTask;
    protected virtual Task HandleSuspendMailbox(IContext context) => Task.CompletedTask;
    protected virtual Task HandleResumeMailbox(IContext context) => Task.CompletedTask;
    protected virtual Task HandleWatch(IContext context) => Task.CompletedTask;
    protected virtual Task HandleUnwatch(IContext context) => Task.CompletedTask;
    protected virtual Task HandleTouch(IContext context) => Task.CompletedTask;
    protected virtual Task HandlePoisonPill(IContext context) => Task.CompletedTask;

    #endregion
    
    #region Abstract Methods
    
    protected abstract Task PersistStateAsync(TEntity actorState);
    
    protected abstract Task RecoverStateAsync();

    protected abstract Receive GetBehaviorFromState(TEntity actorState);

    protected static void RespondUnprocessedMessage(IContext context, ILogger logger, string stateName, string? commandName)
    {
        commandName ??= "Unknown";
        logger.LogError($"The command \'{commandName}\' for state {stateName} was not not recognised and went unprocessed.");
        context.Respond(new InvalidCommandForState
        {
            CommandName = commandName,
            ActorState = stateName
        });
    }
    
    #endregion
    
    #region Logging Methods
    
    private static void LogStarted(ILogger logger, string actorId)
        => logger.LogInformation("Actor of type '{ActorType}' with ActorId: '{ActorId}' has started", ActorTypeName, actorId);

    private static void LogReceivedTimeout(ILogger logger, string actorId)
        => logger.LogInformation("Actor of type '{ActorType}' with ActorId: '{ActorId}' has received a timeout request and will stop", ActorTypeName, actorId);

    private static void LogPoisonPill(ILogger logger, string actorId)
        => logger.LogInformation("Actor of type '{ActorType}' with ActorId: '{ActorId}' has been poisoned", ActorTypeName, actorId);
    
    private static void LogRestart(ILogger logger, string actorId, Exception ex)
        => logger.LogInformation("Actor of type '{ActorType}' with ActorId: '{ActorId}' is restarting because of exception: '{Exception}'", ActorTypeName, actorId, ex.Message);
    
    private static void LogChildTerminated(ILogger logger, string actorId, PID who, TerminatedReason why)
        => logger.LogInformation("Actor of type '{ActorType}' with ActorId: '{ActorId}' learned that child with Id: '{ChildId}' was terminated because: '{Reason}'", ActorTypeName, actorId, who.Id, why.ToString());
    
    private static void LogRestarting(ILogger logger, string actorId)
        => logger.LogInformation("Actor of type '{ActorType}' with ActorId: '{ActorId}' is restarting", ActorTypeName, actorId);
    
    private static void LogStopping(ILogger logger, string actorId)
        => logger.LogInformation("Actor of type '{ActorType}' with ActorId: '{ActorId}' is stopping", ActorTypeName, actorId);
    
    private static void LogStopped(ILogger logger, string actorId)
        => logger.LogInformation("Actor of type '{ActorType}' with ActorId: '{ActorId}' has stopped", ActorTypeName, actorId);
    
    private static void LogFailure(ILogger logger, string actorId, PID who, Exception ex, object? message)
        => logger.LogError(ex,"Actor of type '{ActorType}' with ActorId: '{ActorId}' learned that child with Id: '{ChildId}' failed with message: '{Message}'", ActorTypeName, actorId, who.Id, ex.Message);
    
    private static void LogSuspendedMailbox(ILogger logger, string actorId)
        => logger.LogInformation("Actor of type '{ActorType}' with ActorId: '{ActorId}' has had its mailbox suspended", ActorTypeName, actorId);
    
    private static void LogResumedMailbox(ILogger logger, string actorId)
        => logger.LogInformation("Actor of type '{ActorType}' with ActorId: '{ActorId}' has had its mailbox resumed", ActorTypeName, actorId);

    private static void LogWatch(ILogger logger, string actorId, string watcherId)
        => logger.LogInformation("Actor of type '{ActorType}' with ActorId: '{ActorId}' is being watched by Actor with Id: {WatcherID}", ActorTypeName, actorId,watcherId);
    
    private static void LogUnwatch(ILogger logger, string actorId, string watcherId)
        => logger.LogInformation("Actor of type '{ActorType}' with ActorId: '{ActorId}' is no longer watched by Actor with Id: {WatcherID}", ActorTypeName, actorId,watcherId);
    
    private static void LogTouch(ILogger logger, string actorId)
        => logger.LogInformation("Actor of type '{ActorType}' with ActorId: '{ActorId}' has received a Touch", ActorTypeName, actorId);
    
    #endregion
}

[ProtoContract]
public record UninitializedEntityActorError([property:ProtoMember(1)]string Message);

[ProtoContract]
public record InvalidCommandForState : IResponse
{
    [ProtoMember(1)]
    public required string CommandName { get; init; }
    [ProtoMember(2)]
    public required string ActorState { get; init; }
}