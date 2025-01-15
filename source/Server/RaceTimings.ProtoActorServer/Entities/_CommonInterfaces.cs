namespace RaceTimings.ProtoActorServer.Entities;


public interface IEntityWithId<out TKey> where TKey : notnull, allows ref struct
{
    public TKey Id { get; }
}

public interface IMutableEntity
{
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}