using MQTTnet;
using MQTTnet.Client;
using Microsoft.Extensions.Logging;
using Proto;
using ProtoBuf;

namespace RaceTimings.ProtoActorServer.Actors;

public class MqttSubscriberActor(ActorDependencyResolver actorDependencyResolver,
    ILogger<MqttSubscriberActor> logger,
    Guid subscriberId, 
    string brokerHost, 
    string topicFilter,
    int? brokerPortNumber = null) : IActor
{
    private readonly IMqttClient _mqttClient = new MqttFactory().CreateMqttClient();
    private IContext? _actorContext;
    private PID? _routerActor;
    private MqttClientOptions? _clientOptions;
    private MqttClientSubscribeOptions? _subOptions;
    private ulong _messageCount;

    public async Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case Started:
                _routerActor = context.Spawn(actorDependencyResolver.CreateProps<MqttRouterActor>());
                _actorContext = context;
                await InitializeMqttClient();
                break;
            case IoTMessage:
                logger.LogInformation($"MqqtSubscriberActor:{subscriberId} - MQTT Message Received - Topic: {++_messageCount}");
                if (_routerActor is not null) 
                    context.Forward(_routerActor);
                break;
            case PoisonPill _:
                logger.LogInformation($"MqqtSubscriberActor:{subscriberId} - MQTT Listener Actor is stopping...");
                await StopMqttClient();
                if (_routerActor is not null)
                {
                    await context.PoisonAsync(_routerActor);
                    _routerActor = null;
                }
                break;
            case Stopping:
                logger.LogInformation($"MqqtSubscriberActor:{subscriberId} - MQTT Listener Actor is stopping...");
                break;
            
            case Stopped:
                logger.LogInformation($"MqqtSubscriberActor:{subscriberId} - MQTT Listener Actor is stopped!");
                break;

            default:
                logger.LogInformation($"MqqtSubscriberActor:{subscriberId} - Unhandled message: {context.Message}");
                break;
        }
    }

    private async Task InitializeMqttClient()
    {
        // Configure the MQTT client options
        _clientOptions = new MqttClientOptionsBuilder()
            .WithClientId($"MqttSubscriberActor-{subscriberId}")
            .WithTcpServer(brokerHost, brokerPortNumber) // Update for your NanoMQ broker's address and port
            .WithCleanSession()
            .Build();

        _subOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(topicFilter, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce) // QoS Level 1
            .Build();
        
        _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceived;
        _mqttClient.DisconnectedAsync += OnDisconnected;

        // Connect to the MQTT broker
        try
        {
            await _mqttClient.ConnectAsync(_clientOptions);
            await _mqttClient.SubscribeAsync(_subOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to MQTT broker: {ex.Message}");
        }
    }

    private async Task OnDisconnected(MqttClientDisconnectedEventArgs arg)
    {
        if(_clientOptions is null || _subOptions is null)
            return;
        
        var retryCount = 3;
        while (retryCount-- > 0)
        {
            try
            {
                await _mqttClient.ConnectAsync(_clientOptions);
                await _mqttClient.SubscribeAsync(_subOptions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"MqqtSubscriberActor:{subscriberId} - Failed to reconnect to MQTT broker. Error: {ex.Message}");
                if(retryCount == 0)
                    throw;
            }

            await Task.Delay(2000);
        }
    }

    private async Task StopMqttClient()
    {
        try
        {
            _mqttClient.DisconnectedAsync -= OnDisconnected;
            await _mqttClient.DisconnectAsync();
            _mqttClient.Dispose();
            logger.LogInformation($"MqqtSubscriberActor:{subscriberId} - MQTT client disconnected and disposed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"MqqtSubscriberActor:{subscriberId} - Error while stopping MQTT client. Error: {ex.Message}");
            throw;
        }
    }
    
    private Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs args)
    {
        
        // Process the incoming MQTT message
        var topic = args.ApplicationMessage.Topic;
        var payload = args.ApplicationMessage.PayloadSegment.Array;


        if (topic is null || payload is null) 
            return Task.CompletedTask;
        
        _actorContext?.Send(_actorContext.Self,new IoTMessage(topic, payload));

        // You can send this message to other actors or process it directly
        return Task.CompletedTask;
    }
}


[ProtoContract]
public record IoTMessage([property: ProtoMember(1)]string Topic, [property:ProtoMember(2)] byte[] Payload);

