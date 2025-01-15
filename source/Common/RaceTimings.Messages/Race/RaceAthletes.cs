using ProtoBuf;

namespace RaceTimings.Messages;

public interface IRaceAddAthleteResponse: IResponse;
public interface IRaceRemoveAthleteResponse: IResponse;
public interface IRaceSwapAthleteLanesResponse: IResponse;

[ProtoContract]
public record RaceAddAthlete
{
    [ProtoMember(1)]
    public Guid RaceId { get; init; }
    [ProtoMember(2)]
    public Guid AthleteId { get; init; }
    [ProtoMember(3)]
    public short LaneNumber { get; init; }
}

[ProtoContract]
public record RaceAddAthleteSuccess: IRaceAddAthleteResponse;

[ProtoContract]
public record RaceAddAthleteFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)]string Error): IRaceAddAthleteResponse, IFailureResponse;

[ProtoContract]
public record RaceRemoveAthleteRequest: IRequest
{
    [ProtoMember(1)]
    public Guid RaceId { get; init; }
    [ProtoMember(2)]
    public Guid AthleteId { get; init; }
}

[ProtoContract]
public record RaceRemoveAthleteSuccess: IRaceRemoveAthleteResponse;

[ProtoContract]
public record RaceRemoveAthleteFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error): IRaceRemoveAthleteResponse, IFailureResponse;

[ProtoContract]
public record RaceSwapAthleteLanesRequest
{
    [ProtoMember(1)]
    public short Origin { get; init; }
    
    [ProtoMember(2)]
    public short Destination { get; init; }
}

[ProtoContract]
public record RaceSwapAthleteLanesSuccess: IRaceSwapAthleteLanesResponse;

[ProtoContract]
public record RaceSwapAthleteLanesFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error) : IRaceSwapAthleteLanesResponse;