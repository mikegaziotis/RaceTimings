using ProtoBuf;
using RaceTimings.Messages;

namespace RaceTimings.ProtoActorServer.Entities;


//Entity Type
[ProtoContract]
public record RaceEntity: IEntityWithId<Guid>, IMutableEntity, IRace
{
    [ProtoMember(1)]
    public Guid Id { get; init; }
    
    [ProtoMember(2)]
    public required string Name { get; init; }
    
    [ProtoMember(3)]
    public RaceDistance Distance { get; init; }
    
    [ProtoMember(4)]
    public GenderDivision GenderDivision { get; init; }
    
    [ProtoMember(5)]
    public RaceType Type { get; init; }
    
    [ProtoMember(6)]
    public RaceRound Round { get; init; }
    
    [ProtoMember(7)]
    public required string Location { get; init; }
    
    [ProtoMember(8)]
    public DateTimeOffset? StartDateTime { get; init; }
    
    [ProtoMember(9)]
    public RaceStatus Status { get; init; }
    
    [ProtoMember(10)]
    public short LaneCount { get; init; }
    
    [ProtoMember(11)]
    public DateTimeOffset CreatedAt { get; init; }
    
    [ProtoMember(12)]
    public DateTimeOffset LastUpdatedAt { get; init; }

    #region Relations

    [ProtoIgnore]
    public virtual ICollection<RaceAthleteEntity> Athletes { get; init; } = [];
    
    [ProtoIgnore]
    public virtual ICollection<RaceDeviceEntity> Devices { get; init; } = [];

    [ProtoIgnore] 
    public virtual ICollection<RaceAthleteSplitEntity> AthleteSplits { get; init; } = [];
    
    [ProtoIgnore] 
    public virtual ICollection<RaceAthleteResultEntity> Results { get; init; } = [];

    [ProtoIgnore] 
    public virtual ICollection<RaceAthleteStatsEntity> Stats { get; init; } = [];
    
    [ProtoIgnore] 
    public virtual ICollection<RaceAthleteFalseStartEntity> FalseStarts { get; init; } = [];
    #endregion
    
    
    public Race ToRace() => new()
    {
        Id = Id,
        Name = Name,
        Distance = Distance,
        GenderDivision = GenderDivision,
        Type = Type,
        Round = Round,
        Location = Location,
        StartDateTime = StartDateTime,
        Status = Status,
        LaneCount = LaneCount
    };
}