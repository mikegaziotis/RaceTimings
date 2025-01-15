using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Proto;
using RaceTimings.Messages;
using RaceTimings.Extensions;
using RaceTimings.ProtoActorServer.Entities;
using RaceTimings.ProtoActorServer.Helpers;
using RaceTimings.ProtoActorServer.Providers;
using RaceTimings.ProtoActorServer.Repositories;

namespace RaceTimings.ProtoActorServer;

public class AthleteActor(
    Guid actorId,
    ILogger<AthleteActor> logger,
    IEntityRepository repository,
    IDateTimeProvider dateTimeProvider,
    int? maxIdleTimeMinutes = null): BaseEntityActor<AthleteEntity, Guid>(actorId, logger, repository, maxIdleTimeMinutes)
{
    protected override async Task Uninitialized(IContext context)
    {
        switch (context.Message)
        {
            case AddNewAthleteRequest r:
                await HandleUncertainStateChange(context,
                    () => AddNewAthleteEntity(r, ActorId, dateTimeProvider),
                    () => new AddNewAthleteSuccess(ActorId),
                    errorCode => new AddNewAthleteFailure((int)errorCode, ErrorRegistry.Get(errorCode)));
                break;
            default:
                RespondUnprocessedMessage(context, Logger, nameof(Uninitialized), context.Message?.GetType().Name);
                break;
        }
    }

    private async Task Initialized(IContext context)
    {
        switch (context.Message)
        {
            case UpdateAthleteRequest r:
                await HandleUncertainStateChange(context,
                    () => UpdateAthleteEntity(r, ActorId, ActorState.Value, dateTimeProvider),
                    () => new UpdateAthleteSuccess(),
                    errorCode => new UpdateAthleteFailure((int)errorCode, ErrorRegistry.Get(errorCode)));
                break;
            case ArchiveAthleteRequest:
                await HandleUncertainStateChange(context, 
                    () => ActorState.Value with { LastUpdatedAt = dateTimeProvider.UtcNow },
                    () => new ArchiveAthleteSuccess(),
                    errorCode => new ArchiveAthleteFailure((int)errorCode, ErrorRegistry.Get(errorCode)));
                await context.StopAsync(context.Self);
                break;
            case GetAthleteRequest:
                context.Respond(new GetAthleteResponse(ActorState.Value.ToAthlete()));
                break;
            default:
                RespondUnprocessedMessage(context, Logger, nameof(Initialized), context.Message?.GetType().Name);
                break;
        }
    }

    private static Result<AthleteEntity, ErrorCode> UpdateAthleteEntity(IAthlete athlete, Guid actorId,
        AthleteEntity currentState, IDateTimeProvider dateTimeProvider)
    {
        return AddNewAthleteEntity(athlete, actorId, dateTimeProvider)
            .Bind(newState => CheckSameSex(newState, currentState));

        Result<AthleteEntity, ErrorCode> CheckSameSex(IAthlete stateNew, AthleteEntity stateCurrent)
        {
            if (stateNew.Sex != stateCurrent.Sex)
                return ErrorCode.AthleteCantChangeSex;
            return stateCurrent with
            {
                Name = stateNew.Name,
                Surname = stateNew.Surname,
                CountryId = stateNew.CountryId,
                DateOfBirth = stateNew.DateOfBirth,
                LastUpdatedAt = dateTimeProvider.UtcNow,
            };
        }
    }

    private static Result<AthleteEntity, ErrorCode> AddNewAthleteEntity(IAthlete athlete, Guid actorId,
        IDateTimeProvider dateTimeProvider)
    {
        if (!athlete.DateOfBirth.IsBetween(1, 99))
            return ErrorCode.AthleteInvalidAge;
        string[] names = [athlete.Name, athlete.Surname];
        if (!names.All(x => x.Length.IsBetween(2, 20) && x.MatchesRegexPattern(@"^[\p{L}'-]+$")))
            return ErrorCode.AthleteInvalidName;
        if (!Enum.IsDefined(athlete.Sex))
            return ErrorCode.AthleteInvalidSex;
        var utcNow = dateTimeProvider.UtcNow;
        return CountryData.Countries.Any(x => x.Id == athlete.CountryId)
            ? new AthleteEntity
            {
                Id = actorId,
                Name = athlete.Name,
                Surname = athlete.Surname,
                CountryId = athlete.CountryId,
                DateOfBirth = athlete.DateOfBirth,
                Sex = athlete.Sex,
                CreatedAt = utcNow,
                LastUpdatedAt = utcNow
            }
            : ErrorCode.AthleteInvalidCountry;

    }
    
    protected override Receive GetBehaviorFromState(AthleteEntity actorState) => Initialized;

    protected override Maybe<Guid> TryGetIdFromString(string keyPart) => IdConverter.TryGetGuidIdFromString(keyPart);
    
}