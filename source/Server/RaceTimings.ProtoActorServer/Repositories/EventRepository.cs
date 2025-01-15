using MongoDB.Driver;

namespace RaceTimings.ProtoActorServer.Stores;

public class EventRepository(IMongoDatabase mongoDatabase)
{
    private readonly IMongoCollection<Event> _eventCollection = mongoDatabase.GetCollection<Event>("Events");
    
    public async Task StoreEventAsync(Event newEvent)
    {
        await _eventCollection.InsertOneAsync(newEvent);
    }
    
    public async Task<List<Event>> GetEventsByActorAsync(string actorId)
    {
        var filter = Builders<Event>.Filter.Eq(e => e.ActorId, actorId);
        var sort = Builders<Event>.Sort.Ascending(e => e.Timestamp); // Can also sort by Version
        return await _eventCollection.Find(filter).Sort(sort).ToListAsync();
    }
    
    public async Task<Event> GetEventByIdAsync(string id)
    {
        var filter = Builders<Event>.Filter.Eq(e => e.Id, id);
        return await _eventCollection.Find(filter).FirstOrDefaultAsync();
    }

    
    public async Task<List<Event>> GetEventsByTypeAsync(string eventType, string? actorId = null)
    {
        var filters = new List<FilterDefinition<Event>>()
        {
            Builders<Event>.Filter.Eq(e => e.EventType, eventType)
        };
        if (actorId is not null)
        {
            filters.Add(Builders<Event>.Filter.Eq(e => e.ActorId, actorId));
        }

        var filter = Builders<Event>.Filter.And(filters);
        var sort = Builders<Event>.Sort.Ascending(e => e.Timestamp);
        return await _eventCollection.Find(filter).Sort(sort).ToListAsync();
    }
}
public record Event
{
    public required string Id { get; init; }           // Unique Identifier for the Event
    public DateTime Timestamp { get; init; } // Timestamp of the event
    public required string ActorId { get; init; }     // Actor or Entity ID related to this event
    public required string EventType { get; init; }   // Type of event (e.g., RaceStarted, UserUpdated)
    public int Version { get; set; }        // Version of the Event for the Actor
    public required object Payload { get; set; }     // Actual Event Data (flexible structure per event type)
}