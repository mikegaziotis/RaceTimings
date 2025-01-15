using ProtoBuf;

namespace RaceTimings.Messages;

//Core Data Interface
public interface IRace
{
    public string Name { get; init; }
    public RaceDistance Distance { get; init; }
    public GenderDivision GenderDivision { get; init; }
    public RaceType Type { get; init; }
    public RaceRound Round { get; init; }
    public string Location { get; init; }
    public DateTimeOffset? StartDateTime { get; init; }
    public RaceStatus Status { get; init; }
    public short LaneCount { get; init; }
}

//Internal Reference Types
[ProtoContract]
public enum RaceRound
{
    [ProtoEnum] Heats = 1,
    [ProtoEnum] QuarterFinal = 2,
    [ProtoEnum] SemiFinal = 3,
    [ProtoEnum] Final = 4
}

[ProtoContract]
public enum RaceDistance
{
    [ProtoEnum] M60 = 60,
    [ProtoEnum] M100 = 100,
    [ProtoEnum] M200 = 200,
    [ProtoEnum] M400 = 400
}

public enum RaceType
{
    [ProtoEnum] Plain = 1,
    [ProtoEnum] Obstacle = 2,
    [ProtoEnum] Relay = 3
}

[ProtoContract]
public enum GenderDivision
{
    [ProtoEnum] Men = 1,
    [ProtoEnum] Women = 2,
    [ProtoEnum] Mixed = 3
}

[ProtoContract]
public enum RaceStatus
{
    [ProtoEnum] Scheduled = 1,
    [ProtoEnum] ReadyToStart = 2,
    [ProtoEnum] Ongoing = 3,
    [ProtoEnum] Finished = 4,
    [ProtoEnum] Canceled = 99
}


[ProtoContract]
public record AthleteRaceLane
{
    [ProtoMember(1)]
    public required Guid AthleteId { get; init; }
    [ProtoMember(2)]
    public required short LaneNumber { get; init; }
}

//Core Data Class
[ProtoContract]
public record Race : IRace
{
    [ProtoMember(1)]
    public required Guid Id { get; init; }
    
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
}

#region CRUD
[ProtoContract]
public record AddNewRaceRequest: IRace, IRequest
{
    [ProtoMember(1)]
    public required string Name { get; init; }
    
    [ProtoMember(2)]
    public RaceDistance Distance { get; init; }
    
    [ProtoMember(3)]
    public GenderDivision GenderDivision { get; init; }
    
    [ProtoMember(4)]
    public RaceType Type { get; init; }
    
    [ProtoMember(5)]
    public RaceRound Round { get; init; }
    
    [ProtoMember(6)]
    public required string Location { get; init; }
    
    [ProtoMember(7)]
    public DateTimeOffset? StartDateTime { get; init; }
    
    [ProtoMember(8)]
    public RaceStatus Status { get; init; }
    
    [ProtoMember(9)]
    public short LaneCount { get; init; }

}

public interface IAddNewRaceResponse: IResponse;

[ProtoContract]
public record AddNewRaceSuccess([property: ProtoMember(1)] Guid Id):IAddNewRaceResponse;

[ProtoContract]
public record AddNewRaceFailure([property:ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error):IAddNewRaceResponse, IFailureResponse;

[ProtoContract]
public record UpdateRaceRequest: IRace,IRequest 
{
    [ProtoMember(1)]
    public required string Name { get; init; }
    
    [ProtoMember(2)]
    public RaceDistance Distance { get; init; }
    
    [ProtoMember(3)]
    public GenderDivision GenderDivision { get; init; }
    
    [ProtoMember(4)]
    public RaceType Type { get; init; }
    
    [ProtoMember(5)]
    public RaceRound Round { get; init; }
    
    [ProtoMember(6)]
    public required string Location { get; init; }
    
    [ProtoMember(7)]
    public DateTimeOffset? StartDateTime { get; init; }
    
    [ProtoMember(8)]
    public RaceStatus Status { get; init; }
    
    [ProtoMember(9)]
    public short LaneCount { get; init; }
}

public interface IUpdateRaceResponse: IResponse;

[ProtoContract]
public record UpdateRaceSuccess([property: ProtoMember(1)] Guid Id): IUpdateRaceResponse;

[ProtoContract]
public record UpdateRaceFailure([property:ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error): IUpdateRaceResponse, IFailureResponse;

[ProtoContract]
public record GetRaceStatusRequest:IRequest
{
    [ProtoMember(1)]
    public required Guid RaceId { get; init; }
}

[ProtoContract]
public interface IGetRaceStatusResponse : IResponse;

[ProtoContract]
public record GetRaceStatusSuccess([property: ProtoMember(1)] RaceStatus Status): IGetRaceStatusResponse;

[ProtoContract]
public record GetRaceStatusFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error): IGetRaceStatusResponse, IFailureResponse;

[ProtoContract]
public record ArchiveRaceRequest([property: ProtoMember(1)] Guid Id): IRequest;

public interface IArchiveRaceResponse: IResponse;

[ProtoContract]
public record ArchiveRaceSuccess([property: ProtoMember(1)] Guid Id): IArchiveRaceResponse;

[ProtoContract]
public record ArchiveRaceFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error): IArchiveRaceResponse, IFailureResponse;

[ProtoContract]
public record GetRaceRequest([property: ProtoMember(1)] Guid Id): IRequest;

public interface IGetRaceResponse: IResponse;

[ProtoContract]
public record GetRaceSuccess([property: ProtoMember(1)] Race Race): IGetRaceResponse;

[ProtoContract]
public record GetRaceFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error): IGetRaceResponse, IFailureResponse;

#endregion