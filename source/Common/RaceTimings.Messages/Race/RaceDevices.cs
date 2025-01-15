using ProtoBuf;

namespace RaceTimings.Messages;

[ProtoContract]
public record RaceAddOrUpdateStartingGun : IRequest
{
    [ProtoMember(1)]
    Guid RaceId { get; init; }
    [ProtoMember(2)]
    Guid StartingGunDeviceId {get; init;}
}

[ProtoContract]
public record RaceAddDevicesToLaneRequest: IRequest
{
    [ProtoMember(1)]
    public Guid RaceId { get; init; }
    [ProtoMember(2)]
    public short Lane { get; init; }
    [ProtoMember(3)]
    Guid ReactionTimeDeviceId {get; init;}
    [ProtoMember(4)]
    Guid DistanceSensorDeviceId {get; init;}
    [ProtoMember(5)]
    Guid FinishLineSensorDeviceId {get; init;}
}

public interface IRaceAddDevicesToLaneResponse: IResponse;

[ProtoContract]
public record RaceAddDevicesToLaneSuccess : IRaceAddDevicesToLaneResponse;

[ProtoContract]
public record RaceAddDevicesToLaneFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error): IRaceAddDevicesToLaneResponse, IFailureResponse;

[ProtoContract]
public record RaceRemoveDevicesFromLaneRequest([property:ProtoMember(1)] short Lane): IRequest;

[ProtoContract]
public record RaceRemoveDevicesFromLane([property:ProtoMember(1)] short Lane): IRequest;

public interface IRaceRemoveDevicesFromLaneResponse: IResponse;

[ProtoContract]
public record RaceRemoveDevicesFromLaneSuccess: IRaceRemoveDevicesFromLaneResponse;

[ProtoContract]
public record RaceRemoveDevicesFromLaneFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error): IRaceRemoveDevicesFromLaneResponse, IFailureResponse;
