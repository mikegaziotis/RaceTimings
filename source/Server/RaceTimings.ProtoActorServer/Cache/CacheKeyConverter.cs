using CSharpFunctionalExtensions;

namespace RaceTimings.ProtoActorServer.Cache;

public static class CacheKeyConverter
{
    public static string GetEntityName<TEntity>() => typeof(TEntity).Name.Replace("Entity", "").ToLower();
    
    public static string GetEntityStoreKey<TEntity,TKey>(TKey entityId) => $"{GetEntityName<TEntity>()}:{entityId}";
    public static string GetEntityKeyCollection<TEntity>() => $"Keys:{GetEntityName<TEntity>()}";
    
    public static string GetPIDStoreKey<TEntity,TKey>(TKey entityId) => $"PID:{GetEntityName<TEntity>()}:{entityId}";
    
    public static Maybe<Guid> TryGetGuidIdFromString(string keyPart) => Guid.TryParse(keyPart, out var id) ? id : Maybe<Guid>.None;
    public static Maybe<string> TryGetStringIdFromString(string keyPart) => string.IsNullOrEmpty(keyPart) ? Maybe<string>.None : keyPart;
    public static Maybe<int> TryGetIntIdFromString(string keyPart) => int.TryParse(keyPart, out var id) ? id : Maybe<int>.None;
    public static Maybe<long> TryGetLongIdFromString(string keyPart) => long.TryParse(keyPart, out var id) ? id : Maybe<long>.None;
}