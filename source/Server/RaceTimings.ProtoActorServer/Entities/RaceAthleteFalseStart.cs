using ProtoBuf;

namespace RaceTimings.ProtoActorServer.Entities;

[ProtoContract]
public record RaceAthleteFalseStartEntity: IEntityWithId<Guid>
{
    [ProtoMember(1)]
    public Guid Id { get; init; }
    
    [ProtoMember(2)]
    public Guid AthleteId { get; init; }
    
    [ProtoMember(3)]
    public Guid RaceId { get; init; }
    
    [ProtoMember(4)]
    public short StartAttempt { get; init; }
    
    [ProtoMember(5)]
    public int ReactionTimeMs { get; init; }
    
    #region Relations
    
    [ProtoIgnore]
    public RaceAthleteStatsEntity RaceAthleteStats { get; init; } = null!;
    
    [ProtoIgnore]
    public AthleteEntity Athlete { get; init; } = null!;
    
    [ProtoIgnore]
    public RaceEntity Race { get; init; } = null!;
    
    #endregion
}