using ProtoBuf;
using RaceTimings.Messages;

namespace RaceTimings.ProtoActorServer.Entities;


//Entity Type
[ProtoContract]
public record AthleteEntity: IEntityWithId<Guid>, IMutableEntity, IAthlete
{
    [ProtoMember(1)]
    public required Guid Id { get; init; }
    
    [ProtoMember(2)]
    public required string Name { get; init; }
    
    [ProtoMember(3)]
    public required string Surname { get; init; }
    
    [ProtoMember(4)]
    public required string CountryId { get; init; }
    
    [ProtoMember(5)]
    public Sex Sex { get; init; }
    
    [ProtoMember(6)]
    public DateTimeOffset DateOfBirth { get; init; }
    
    [ProtoMember(7)]
    public DateTimeOffset CreatedAt { get; init; }
    
    [ProtoMember(8)]
    public DateTimeOffset LastUpdatedAt { get; init; }
    
    #region Relations
    
    [ProtoIgnore]
    public virtual ICollection<RaceAthleteEntity> RaceAthletes { get; init; } = [];
    
    [ProtoIgnore]
    public virtual ICollection<RaceAthleteSplitEntity> Splits { get; init; } = [];
    
    [ProtoIgnore]
    public virtual ICollection<RaceAthleteFalseStartEntity> RaceFalseStarts { get; init; } = [];
    
    [ProtoIgnore]
    public virtual ICollection<RaceAthleteStatsEntity> RaceStats { get; init; } = [];
    
    [ProtoIgnore]
    public virtual ICollection<RaceAthleteResultEntity> RaceResults { get; init; } = [];
    
    #endregion
    
    public Athlete ToAthlete() => new()
    {
        Id = Id,
        Name = Name,
        Surname = Surname,
        DateOfBirth = DateOfBirth,
        CountryId = CountryId,
        Sex = Sex
    };
};