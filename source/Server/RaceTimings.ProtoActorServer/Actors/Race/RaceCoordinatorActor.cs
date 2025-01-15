using Microsoft.Extensions.Logging;
using Proto;
using RaceTimings.Messages;
using RaceTimings.ProtoActorServer.Entities;
using ShortRace = (int RaceId, string RaceName);

namespace RaceTimings.ProtoActorServer.Actors;

public class RaceCoordinatorActor(
    ActorDependencyResolver actorDependencyResolver, 
    ILogger<RaceCoordinatorActor> logger): IActor
{
    private readonly List<ShortRace> _races = [];
    
    public Task ReceiveAsync(IContext context)
    {
        throw new NotImplementedException();
    }

    private void CreateRace(AddNewRaceRequest addNewRace)
    {
        if(_races.Any(x=>x.RaceName==addNewRace.Name))
        {
            // RaceEntityActor already exists
            return;
        }
        var id = Guid.NewGuid();
        var race = new RaceEntity
        {
            Id = id,
            Name = addNewRace.Name,
            Location = addNewRace.Location,
            Status = RaceStatus.Scheduled
        };
        
    }
}