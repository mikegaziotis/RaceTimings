using System.ComponentModel.DataAnnotations;
using ProtoBuf;

namespace RaceTimings.ProtoActorServer.Entities;

[ProtoContract]
public record RaceAthleteEntity: IEntityWithId<string>, IMutableEntity
{
    [ProtoMember(1)]
    public string Id => $"{RaceId}/{AthleteId}";
    
    [ProtoMember(2)] [Key]
    public Guid RaceId { get; init; }
    
    [ProtoMember(3)] [Key]
    public Guid AthleteId { get; init; }
    
    [ProtoMember(4)]
    public int Lane { get; init; }
    
    [ProtoMember(5)]
    public DateTimeOffset CreatedAt { get; init; }
    
    [ProtoMember(6)]
    public DateTimeOffset LastUpdatedAt { get; init; }
    
    #region Relations
    
    [ProtoIgnore]
    public RaceEntity Race { get; init; } = null!;
    
    [ProtoIgnore]
    public AthleteEntity Athlete { get; init; } = null!;
    
    [ProtoIgnore]
    public RaceAthleteResultEntity Result { get; init; } = null!;
    
    [ProtoIgnore]
    public RaceAthleteStatsEntity Stats { get; init; } = null!;

    #endregion
    
}

