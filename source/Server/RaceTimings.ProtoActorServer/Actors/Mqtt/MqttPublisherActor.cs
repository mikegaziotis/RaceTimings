using MQTTnet;
using MQTTnet.Client;
using Microsoft.Extensions.Logging;
using Proto;
using ProtoBuf;

namespace RaceTimings.ProtoActorServer.Actors;

public class MqttPublisherActor(ActorDependencyResolver actorDependencyResolver,
    ILogger<MqttPublisherActor> logger,
    Guid publisherId, 
    string brokerHost, 
    string topic,
    int? brokerPortNumber = null) : IActor
{
    private readonly IMqttClient _mqttClient = new MqttFactory().CreateMqttClient();
    private IContext? _actorContext;
    private PID? _routerActor;
    private MqttClientOptions? _clientOptions;
    private ulong _messageCount;

    public async Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case Started:
                await InitializeMqttClient();
                break;
            case PublishedMessage msg:
                await _mqttClient.PublishAsync(new MqttApplicationMessage
                {
                    Topic = topic,
                    PayloadSegment = msg.Payload,
                    QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                    Retain = false
                });
                break;
            case PoisonPill _:
                logger.LogInformation($"MqqtPublisherActor:{publisherId} - MQTT Listener Actor is stopping...");
                await StopMqttClient();
                if (_routerActor is not null)
                {
                    await context.PoisonAsync(_routerActor);
                    _routerActor = null;
                }
                break;
            case Stopping:
                logger.LogInformation($"MqqtPublisherActor:{publisherId} - MQTT Listener Actor is stopping...");
                break;
            
            case Stopped:
                logger.LogInformation($"MqqtPublisherActor:{publisherId} - MQTT Listener Actor is stopped!");
                break;

            default:
                logger.LogInformation($"MqqtPublisherActor:{publisherId} - Unhandled message: {context.Message}");
                break;
        }
    }

    private async Task InitializeMqttClient()
    {
        // Configure the MQTT client options
        _clientOptions = new MqttClientOptionsBuilder()
            .WithClientId($"MqttPublisher-{publisherId}")
            .WithTcpServer(brokerHost, brokerPortNumber) // Update for your NanoMQ broker's address and port
            .WithCleanSession()
            .Build();

        _mqttClient.DisconnectedAsync += OnDisconnected;

        // Connect to the MQTT broker
        try
        {
            await _mqttClient.ConnectAsync(_clientOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to MQTT broker: {ex.Message}");
        }
    }

    private async Task OnDisconnected(MqttClientDisconnectedEventArgs arg)
    {
        if(_clientOptions is null)
            return;
        
        var retryCount = 3;
        while (retryCount-- > 0)
        {
            try
            {
                await _mqttClient.ConnectAsync(_clientOptions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"MqqtPublisherActor:{publisherId} - Failed to reconnect to MQTT broker. Error: {ex.Message}");
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
            logger.LogInformation($"MqqtPublisherActor:{publisherId} - MQTT client disconnected and disposed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"MqqtPublisherActor:{publisherId} - Error while stopping MQTT client. Error: {ex.Message}");
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

public record PublishedMessage(string Topic, byte[] Payload);


