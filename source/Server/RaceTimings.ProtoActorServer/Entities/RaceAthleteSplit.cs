using ProtoBuf;

namespace RaceTimings.ProtoActorServer.Entities;

[ProtoContract]
public record RaceAthleteSplitEntity: IEntityWithId<Guid>
{
    [ProtoMember(1)]
    public Guid Id { get; init; }
    
    [ProtoMember(2)] 
    public Guid RaceId { get; init; }
    
    [ProtoMember(3)] 
    public Guid AthleteId { get; init; }
    
    [ProtoMember(4)]
    public int DistanceCm { get; init; }
    
    [ProtoMember(5)]
    public int TimeMs { get; init; }
    
    [ProtoIgnore]
    public RaceAthleteStatsEntity RaceAthleteStats { get; set; } = null!;
    
    [ProtoIgnore]
    public RaceEntity Race { get; set; } = null!;
    
    [ProtoIgnore]
    public AthleteEntity Athlete { get; set; } = null!;
}