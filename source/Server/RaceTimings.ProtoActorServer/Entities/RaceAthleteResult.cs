using System.ComponentModel.DataAnnotations;
using ProtoBuf;

namespace RaceTimings.ProtoActorServer.Entities;

[ProtoContract]
public class RaceAthleteResultEntity : IEntityWithId<string>, IMutableEntity
{
    [ProtoIgnore]
    public string Id => $"{RaceId}/{AthleteId}";
    
    [ProtoMember(1)] [Key]
    public Guid RaceId { get; init; }
    
    [ProtoMember(2)] [Key]
    public Guid AthleteId { get; init; }
    
    [ProtoMember(3)]
    public bool CompletedRace { get; init; }
    
    [ProtoMember(4)]
    public RaceNonCompletionReason? NonCompletionReason { get; init; }
    
    [ProtoMember(5)]
    public Medal? Medal { get; init; }
    
    [ProtoMember(6)]
    public DateTimeOffset CreatedAt { get; init; }
    
    [ProtoMember(7)]
    public DateTimeOffset LastUpdatedAt { get; init; }
    
    [ProtoMember(8)]
    public bool IsArchived { get; init; }
    
    #region Relations
    
    [ProtoIgnore]
    public virtual RaceAthleteEntity RaceAthlete { get; init; } = null!;
    
    [ProtoIgnore]
    public virtual RaceEntity Race { get; init; } = null!;
    
    [ProtoIgnore]
    public virtual AthleteEntity Athlete { get; init; } = null!;
    
    #endregion
}

[ProtoContract]
public enum RaceNonCompletionReason
{
    [ProtoEnum]
    Disqualified =1,
    [ProtoEnum]
    Retired = 2,
    [ProtoEnum]
    Unknown = 99
}

[ProtoContract]
public enum Medal
{
    [ProtoEnum]
    Gold = 1,
    [ProtoEnum]
    Silver = 2,
    [ProtoEnum]
    Bronze = 3
}
