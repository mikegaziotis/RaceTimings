using ProtoBuf;
using RaceTimings.Messages.Device;

namespace RaceTimings.ProtoActorServer.Entities;


//Entity Type
[ProtoContract]
public record DeviceEntity: IEntityWithId<Guid>,IMutableEntity, IDevice
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
    
    [ProtoMember(8)]
    public DateTimeOffset CreatedAt { get; init; }
    
    [ProtoMember(9)]
    public DateTimeOffset LastUpdatedAt { get; init; }
    
    #region Relations
    [ProtoIgnore]
    public virtual ICollection<RaceDeviceEntity> RaceDevices { get; init; } = [];

    #endregion
    
    public Device ToDevice() => new()
    {
        Id = Id,
        Type = Type,
        ManufactureDate = ManufactureDate,
        ManufactureLocation = ManufactureLocation,
        Manufacturer = Manufacturer,
        LastServicedAt = LastServicedAt,
        ServiceLocation = ServiceLocation
    };
}
