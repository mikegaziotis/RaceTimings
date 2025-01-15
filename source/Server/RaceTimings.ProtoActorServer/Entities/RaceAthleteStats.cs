using System.ComponentModel.DataAnnotations;
using ProtoBuf;

namespace RaceTimings.ProtoActorServer.Entities;

[ProtoContract]
[Serializable]
public sealed record RaceAthleteStatsEntity: IEntityWithId<string>, IMutableEntity
{
    [ProtoIgnore] 
    public string Id => $"{RaceId}/{AthleteId}";
    
    [ProtoMember(1)] [Key]
    public Guid RaceId { get; init; }
    
    [ProtoMember(2)] [Key]
    public Guid AthleteId { get; init; }
    
    [ProtoMember(3)]
    public int? FinishTimeMs { get; init; }
    
    [ProtoMember(4)]
    public int? ReactionTimeMs { get; init; }
    
    [ProtoMember(5)]
    public double? TopSpeedKmH { get; init; }
    
    [ProtoMember(6)]
    public DateTimeOffset CreatedAt { get; init; }
    
    [ProtoMember(7)]
    public DateTimeOffset LastUpdatedAt { get; init; }
    
    [ProtoMember(8)]
    public bool IsArchived { get; init; }

    #region Relations

    [ProtoIgnore]
    public RaceAthleteEntity RaceAthlete { get; set; } = null!;
    
    [ProtoIgnore]
    public RaceEntity Race { get; set; } = null!;
    
    [ProtoIgnore]
    public AthleteEntity Athlete { get; set; } = null!;
    
    [ProtoMember(9)]
    public ICollection<RaceAthleteFalseStartEntity> FalseStarts { get; init; } = [];
    
    [ProtoMember(10)]
    public ICollection<RaceAthleteSplitEntity> Splits { get; init; } = [];
    #endregion
    
};




