using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Proto;
using RaceTimings.Messages;
using SubscriberTuple = (Proto.PID, RaceTimings.Messages.Subscriber);
using PublisherKeyTuple = (string BrokerHostAndPort, string Topic);

namespace RaceTimings.ProtoActorServer.Actors;

public class MqttCoordinatorActor(ActorDependencyResolver actorDependencyResolver, 
    ILogger<MqttCoordinatorActor> logger): IActor
{
    private ImmutableDictionary<Guid, SubscriberTuple> _subscribers = ImmutableDictionary<Guid, SubscriberTuple>.Empty;
    private ImmutableDictionary<PublisherKeyTuple, PID> _publishers = ImmutableDictionary<PublisherKeyTuple, PID>.Empty;
    private static string GetBrokerHostAndPort(string brokerHost, int? brokerPort) => brokerHost + (brokerPort is not null ? $":{brokerPort}" : ""); 
    private static PublisherKeyTuple GetPublisherKey(GetOrCreateNewPublisher request) => new(GetBrokerHostAndPort(request.BrokerHost, request.BrokerPortNumber), request.Topic);
    
    public async Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case CreateNewSubscriber msg:
                CreateNewSubscriber(context, msg);
                break;
            case GetOrCreateNewPublisher msg:
                GetOrCreateNewPublisher(context, msg);
                break;
            case StopSubscriber msg:
                if (_subscribers.TryGetValue(msg.SubscriberId, out var subscriberToStop))
                {
                    await context.PoisonAsync(subscriberToStop.Item1);
                    _subscribers = _subscribers.Remove(msg.SubscriberId);
                }
                else
                {
                    logger.LogError($"Subscriber with id {msg.SubscriberId} not found");
                }
                break;
            case GetSubscriber msg:
                if (_subscribers.TryGetValue(msg.SubscriberId, out var subscriberToGet))
                {
                    context.Respond(subscriberToGet.Item2);    
                }
                else
                {
                    logger.LogError($"Subscriber with id {msg.SubscriberId} not found");
                    context.Respond(new SubscriberNotfound());
                }
                break;
            case GetAllSubscribers:
                context.Respond(new AllSubscribers(_subscribers.Values.Select(x=>x.Item2).ToArray()));
                break;
            case PoisonPill:
                foreach (var item in _subscribers)
                {
                    await context.PoisonAsync(item.Value.Item1);
                }
                foreach (var item in _publishers)
                {
                    await context.PoisonAsync(item.Value);
                }
                _subscribers = _subscribers.Clear();
                break;
            default:
                logger.LogInformation($"Unhandled message: {context.Message}");
                break;
        }
    }

    private void CreateNewSubscriber(IContext context, CreateNewSubscriber request)
    {
        
        Props props;
        var subscriberId = Guid.NewGuid();

        if (request.BrokerPortNumber is not null)
            props = actorDependencyResolver.CreateProps<MqttSubscriberActor>(subscriberId, request.TopicFilter,
                request.BrokerHost, request.BrokerPortNumber);
        else
            props = actorDependencyResolver.CreateProps<MqttSubscriberActor>(subscriberId, request.TopicFilter, request.BrokerHost);
        
        var subscriberPid = context.SpawnNamed(props, $"MqttSubscriberActor-{Guid.NewGuid()}");
        _subscribers = _subscribers.Add(subscriberId, (subscriberPid, new Subscriber(subscriberId, request.TopicFilter, request.BrokerHost, request.BrokerPortNumber)));
        
    }

    private void GetOrCreateNewPublisher(IContext context, GetOrCreateNewPublisher request)
    {
        var actorId = GetPublisherKey(request);
        if (_publishers.TryGetValue(actorId, out var publisherPid))
        {
            context.Respond(publisherPid);
            return;
        }

        var props = request.BrokerPortNumber is not null 
            ? actorDependencyResolver.CreateProps<MqttPublisherActor>(request.BrokerHost, request.Topic, request.BrokerPortNumber) 
            : actorDependencyResolver.CreateProps<MqttPublisherActor>(request.BrokerHost, request.Topic);
        
        var newPid = context.Spawn(props);
        _publishers = _publishers.Add(actorId, newPid);
    }
}
