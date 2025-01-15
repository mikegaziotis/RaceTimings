using ProtoBuf;

namespace RaceTimings.ProtoActorServer.Entities;

[ProtoContract]
public record RaceDeviceEntity: IEntityWithId<Guid>, IMutableEntity
{
    [ProtoMember(1)]
    public Guid Id { get; init; }
    
    [ProtoMember(2)]
    public Guid RaceId { get; init; }
    
    [ProtoMember(3)]
    public required string DeviceId { get; init; }
    
    [ProtoMember(4)]
    public int? Lane { get; init; }
    
    [ProtoMember(5)]
    public DateTimeOffset CreatedAt { get; init; }
    
    [ProtoMember(6)]
    public DateTimeOffset LastUpdatedAt { get; init; }
    
    #region Relations
    
    [ProtoIgnore]
    public virtual RaceEntity Race { get; init; } = null!;
    
    [ProtoIgnore]
    public virtual DeviceEntity Device { get; init; } = null!;
    
    #endregion
}