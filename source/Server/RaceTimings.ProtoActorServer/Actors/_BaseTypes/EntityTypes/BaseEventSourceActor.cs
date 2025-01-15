using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Persistence;
using RaceTimings.Messages;
using RaceTimings.ProtoActorServer.Entities;

namespace RaceTimings.ProtoActorServer;

public abstract class BaseEventSourceActor<TEntity, TKey>: AbstractStatefulActor<TEntity,TKey> where TEntity: class, IEntityWithId<TKey> where TKey : notnull
{

    protected readonly Persistence Persistence;
    

    protected BaseEventSourceActor(ILogger logger, IEventStore eventStore, TKey actorId): base(actorId, logger)
    {
        ArgumentNullException.ThrowIfNull(actorId);
        ArgumentNullException.ThrowIfNull(eventStore);

        Persistence = Persistence.WithEventSourcing(eventStore, actorId.ToString()!, ApplyEventHandler);
    }
    
    protected abstract void ApplyEventHandler(Event @event);
   
    public abstract override Task ReceiveAsync(IContext context);
}