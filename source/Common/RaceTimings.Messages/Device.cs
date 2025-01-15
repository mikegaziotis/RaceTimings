using ProtoBuf;

namespace RaceTimings.Messages.Device;

//Core Data Interface
public interface IDevice
{
    public DeviceType Type { get; }
    public DateOnly ManufactureDate { get; }
    public string? ManufactureLocation { get; }
    public string? Manufacturer { get; }
    public DateTimeOffset? LastServicedAt { get; }
    public string? ServiceLocation { get; }
}

//Internal Reference Types
[ProtoContract]
public enum DeviceType
{
    [ProtoEnum]
    StartingGun = 1,
    [ProtoEnum]
    ReactionTimeSensor = 2,
    [ProtoEnum]
    DistanceSensor = 3,
    [ProtoEnum]
    FinishLineSensor = 4
}


//Core Data Class
[ProtoContract]
public record Device: IDevice
{
    [ProtoMember(1)]
    public required Guid Id { get; init; }
    
    [ProtoMember(2)]
    public DeviceType Type { get; init; }
    
    [ProtoMember(3)]
    public DateOnly ManufactureDate { get; init; }
    
    [ProtoMember(4)]
    public string? ManufactureLocation { get; init;}
    
    [ProtoMember(5)]
    public string? Manufacturer { get; init;}
    
    [ProtoMember(6)]
    public DateTimeOffset? LastServicedAt { get; init;}
    
    [ProtoMember(7)]
    public string? ServiceLocation { get; init;}
}

#region CRUD
[ProtoContract]
public record DeviceAddNewRequest: IDevice, IRequest
{
    [ProtoMember(1)]
    public required DeviceType Type { get; init; }
    [ProtoMember(2)]
    public DateOnly ManufactureDate { get; init; }
    [ProtoMember(3)]
    public string? ManufactureLocation { get; init; }
    [ProtoMember(4)]
    public string? Manufacturer { get; init; }
    [ProtoMember(5)]
    public DateTimeOffset? LastServicedAt { get; init; }
    [ProtoMember(6)]
    public string? ServiceLocation { get; init; }
}

public interface IDeviceAddNewResponse: IResponse;

[ProtoContract]
public record DeviceAddNewSuccess([property: ProtoMember(1)] Guid Id): IDeviceAddNewResponse;

[ProtoContract]
public record DeviceAddNewFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error): IDeviceAddNewResponse, IFailureResponse;

[ProtoContract]
public record DeviceUpdateRequest: IDevice, IRequest
{
    [ProtoMember(1)]
    public required Guid Id { get; init; }
    [ProtoMember(2)]
    public required DeviceType Type { get; init; }
    [ProtoMember(3)]
    public DateOnly ManufactureDate { get; init; }
    [ProtoMember(4)]
    public string? ManufactureLocation { get; init; }
    [ProtoMember(5)]
    public string? Manufacturer { get; init; }
    [ProtoMember(6)]
    public DateTimeOffset? LastServicedAt { get; init; }
    [ProtoMember(7)]
    public string? ServiceLocation { get; init; }
}

public interface IDeviceUpdateResponse: IResponse;

[ProtoContract]
public record DeviceUpdateSuccess: IDeviceUpdateResponse;

[ProtoContract]
public record DeviceUpdateFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error): IDeviceUpdateResponse, IFailureResponse;

[ProtoContract]
public record DeviceGetRequest([property: ProtoMember(1)] Guid Id);

public interface IDeviceGetResponse: IResponse;

[ProtoContract]
public record DeviceGetSuccess([property: ProtoMember(1)] Device Device): IDeviceGetResponse;

[ProtoContract]
public record DeviceGetFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error): IDeviceGetResponse, IFailureResponse;

[ProtoContract]
public record DeviceGetAllRequest;

public interface IDeviceGetAllResponse: IResponse;

[ProtoContract]
public record DeviceGetAllSuccess([property: ProtoMember(1)] Device[] Devices): IDeviceGetAllResponse;

[ProtoContract]
public record DeviceGetAllFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error): IDeviceGetAllResponse, IFailureResponse;

[ProtoContract]
public record DeviceArchiveRequest([property: ProtoMember(1)] Guid Id): IRequest;

public interface IDeviceArchiveResponse: IResponse;

[ProtoContract]
public record DeviceArchiveSuccess: IDeviceArchiveResponse;

[ProtoContract]
public record DeviceArchiveFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error): IDeviceArchiveResponse, IFailureResponse;


[ProtoContract]
public record DeviceCheckExistsRequest([property: ProtoMember(1)] Guid Id, [property:ProtoMember(2)] DeviceType Type): IRequest;

public interface IDeviceCheckExistsResponse: IResponse;

[ProtoContract]
public record DeviceCheckExistsSuccess: IDeviceCheckExistsResponse;

[ProtoContract]
public record DeviceCheckExistsFailure([property: ProtoMember(1)] int Code, [property: ProtoMember(2)] string Error): IDeviceCheckExistsResponse, IFailureResponse;

#endregion

#region Events

[ProtoContract]
public record DistanceMeasurementTaken
{
    [ProtoMember(1)]
    public required Guid DeviceId { get; init; }
    [ProtoMember(2)]
    public int DistanceCm { get; init; }
    [ProtoMember(3)]
    public int TimeMs { get; init; }
}

[ProtoContract]
public record FinishLineCrossed
{
    [ProtoMember(1)]
    public required Guid DeviceId { get; init; }
    [ProtoMember(2)]
    public int TimeMs { get; init; }
}

[ProtoContract]
public record StartingGunFired
{
    [ProtoMember(1)]
    public required Guid DeviceId { get; init; }
}
[ProtoContract]
public record ReactionTimeMeasurementTaken
{
    [ProtoMember( 1)]
    public required Guid DeviceId { get; init; }
    [ProtoMember( 2)]
    public int TimeMs { get; init; }
}
#endregion