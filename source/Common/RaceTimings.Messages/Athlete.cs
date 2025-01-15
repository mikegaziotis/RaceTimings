using ProtoBuf;

namespace RaceTimings.Messages;

//Core Data Interface
public interface IAthlete
{
    public string Name { get; }
    public string Surname { get; }
    public string CountryId { get; }
    public Sex Sex { get; }
    public DateTimeOffset DateOfBirth { get; }
}

//Internal Reference Types
[ProtoContract]
public enum Sex
{
    [ProtoEnum] Male,
    [ProtoEnum] Female
}

//Core Data Class
[ProtoContract]
public record Athlete: IAthlete
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
}

#region CRUD

//Add New Request/Response
[ProtoContract]
public record AddNewAthleteRequest: IRequest, IAthlete
{
    [ProtoMember(1)]
    public required string Name { get; init; }
    [ProtoMember(2)]
    public required string Surname { get; init; }
    [ProtoMember(3)]
    public required string CountryId { get; init; }
    [ProtoMember(4)]
    public Sex Sex { get; init; }
    [ProtoMember(5)]
    public DateTimeOffset DateOfBirth { get; init; }
}

public interface IAddNewAthleteResponse: IResponse;

[ProtoContract]
public record AddNewAthleteSuccess([property:ProtoMember(1)] Guid Id): IAddNewAthleteResponse;

[ProtoContract]
public record AddNewAthleteFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error): IAddNewAthleteResponse, IFailureResponse;

//Update Request/Response
[ProtoContract]
public record UpdateAthleteRequest: IRequest, IAthlete
{
    [ProtoMember(1)]
    public required string Name { get; init; }
    
    [ProtoMember(2)]
    public required string Surname { get; init; }
    
    [ProtoMember(3)]
    public required string CountryId { get; init; }
    
    [ProtoMember(4)]
    public Sex Sex { get; init; }
    
    [ProtoMember(5)]
    public DateTimeOffset DateOfBirth { get; init; }
}

public interface IUpdateAthleteResponse:IResponse;
[ProtoContract]
public record UpdateAthleteSuccess:IUpdateAthleteResponse;

[ProtoContract]
public record UpdateAthleteFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error):IUpdateAthleteResponse, IFailureResponse;

// Get Request/Response
[ProtoContract]
public record GetAthleteRequest([property: ProtoMember(1)] Guid Id);

public interface IGetAthleteResponse: IResponse;

[ProtoContract]
public record GetAthleteSuccess([property: ProtoMember(1)] Athlete Athlete): IGetAthleteResponse;

[ProtoContract]
public record GetAthleteFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error): IGetAthleteResponse, IFailureResponse;

// Archive Request/Response
[ProtoContract]
public record ArchiveAthleteRequest([property:ProtoMember(1)] Guid Id): IRequest;

public interface IArchiveAthleteResponse: IResponse;

[ProtoContract]
public record ArchiveAthleteSuccess: IArchiveAthleteResponse;

[ProtoContract]
public record ArchiveAthleteFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error):IArchiveAthleteResponse, IFailureResponse;

#endregion



