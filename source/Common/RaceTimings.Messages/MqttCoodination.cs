using ProtoBuf;

namespace RaceTimings.Messages;


[ProtoContract]
public record CreateNewSubscriber([property:ProtoMember(1)]string TopicFilter, [property:ProtoMember(2)]string BrokerHost, [property:ProtoMember(1)]int? BrokerPortNumber);

[ProtoContract]
public record GetOrCreateNewPublisher([property:ProtoMember(1)]string Topic, [property:ProtoMember(2)]string BrokerHost, [property:ProtoMember(1)]int? BrokerPortNumber);

[ProtoContract]
public record StopSubscriber([property:ProtoMember(1)]Guid SubscriberId);

[ProtoContract]
public record StopPublisher([property:ProtoMember(1)]Guid PublisherId);

[ProtoContract]
public record GetSubscriber([property:ProtoMember(1)]Guid SubscriberId);

[ProtoContract]
public record GetPublisher([property:ProtoMember(1)]Guid SubscriberId);

[ProtoContract]
public record GetAllSubscribers;

[ProtoContract]
public record GetAllPublishers;

[ProtoContract]
public record AllSubscribers([property:ProtoMember(1)] Subscriber[] Subscribers);

[ProtoContract]
public record AllPublishers([property:ProtoMember(1)] Publisher[] Publishers);

[ProtoContract]
public record SubscriberNotfound;

[ProtoContract]
public record PublisherNotfound;

[ProtoContract]
public record Subscriber([property:ProtoMember(1)]Guid SubscriberId, [property:ProtoMember(2)]string TopicFilter, [property:ProtoMember(3)]string BrokerIpAddress, [property:ProtoMember(4)]int? BrokerPortNumber);

public record Publisher([property:ProtoMember(2)]string Topic, [property:ProtoMember(3)]string BrokerIpAddress, [property:ProtoMember(4)]int? BrokerPortNumber);