using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Proto;
using RaceTimings.Messages;
using RaceTimings.Messages.Device;
using RaceTimings.ProtoActorServer.Cache;
using RaceTimings.ProtoActorServer.Entities;
using RaceTimings.ProtoActorServer.Repositories;
using StackExchange.Redis;

namespace RaceTimings.ProtoActorServer;

public sealed class DeviceCoordinatorActor(
    Guid actorId,
    ILogger<DeviceCoordinatorActor> logger, 
    EntityRepository repo): BaseChildlessCoordinatorActor<DeviceEntity,Guid>(actorId, logger, repo)
{
    protected override Maybe<Guid> TryGetIdFromString(string key) => Guid.TryParse(key.Split(':').Last(), out var guid) ? Maybe<Guid>.From(guid) : Maybe<Guid>.None;

    public override async Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case Started when context.Sender is not null:
                logger.LogInformation($"{nameof(DeviceCoordinatorActor)} has received Started message from Child with PID {context.Sender.Id}");
                
                break;
            case DeviceAddNewRequest r:
                await HandleEntityAddRequest<IDeviceAddNewResponse>(context, new DeviceEntity
                    {
                        Id = Guid.CreateVersion7(),
                        Type = r.Type,
                        Manufacturer = r.Manufacturer,
                        ManufactureDate = r.ManufactureDate,
                        ManufactureLocation = r.ManufactureLocation,
                        ServiceLocation = r.ServiceLocation,
                        LastServicedAt = r.LastServicedAt
                    }, 
                    null,
                    entity=> new DeviceAddNewSuccess(entity.Id),
                    errorCode=> new DeviceAddNewFailure((int)errorCode, ErrorRegistry.Get(errorCode)));
                break;
            case DeviceGetRequest r:
                await HandleEntityGetRequest<IDeviceGetResponse>(context, r.Id, 
                    entity=> new DeviceGetSuccess(entity.ToDevice()),
                    errorCode=> new DeviceGetFailure((int)errorCode, ErrorRegistry.Get(errorCode)));
                break;
            case DeviceGetAllRequest:
                await HandleEntitiesGetAllRequest<IDeviceGetAllResponse>(context,
                    entities => new DeviceGetAllSuccess(entities.Select(x => x.ToDevice()).ToArray()),
                    errorCode => new DeviceGetAllFailure((int)errorCode, ErrorRegistry.Get(errorCode)));
                break;
            case DeviceUpdateRequest r:
                await HandleEntityUpdateRequest<IDeviceUpdateResponse>(context, new DeviceEntity
                    {
                        Id = r.Id,
                        Type = r.Type,
                        Manufacturer = r.Manufacturer,
                        ManufactureDate = r.ManufactureDate,
                        ManufactureLocation = r.ManufactureLocation,
                        ServiceLocation = r.ServiceLocation,
                        LastServicedAt = r.LastServicedAt
                    }
                    ,null
                    ,_=> new DeviceUpdateSuccess()
                    ,errorCode=> new DeviceUpdateFailure((int)errorCode, ErrorRegistry.Get(errorCode)));
                break;
            case DeviceArchiveRequest r:
                await HandleArchiveEntityRequest<IDeviceArchiveResponse>(context, r.Id,
                    () => new DeviceArchiveSuccess(), 
                    errorCode => new DeviceArchiveFailure((int)errorCode, ErrorRegistry.Get(errorCode)));
                break;
            case DeviceCheckExistsRequest r:
                await HandleEntityCheckExistsRequest<IDeviceCheckExistsResponse>(context,r.Id,
                    ()=> new DeviceCheckExistsSuccess(), 
                    errorCode=> new DeviceCheckExistsFailure((int)errorCode, ErrorRegistry.Get(errorCode)));
                break;
            default:
                HandleInvalidRequest(context);
                break;
        }
    }

    #region Coordinator Errors

    protected override ErrorCode EntityNotFoundErrorCode => ErrorCode.DeviceNotFound;
    protected override ErrorCode EntityConflictErrorCode => ErrorCode.DeviceAlreadyExists;
    protected override ErrorCode StoreAccessErrorCode => ErrorCode.DeviceStoreAccessError;

    #endregion
    
}