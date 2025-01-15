using ProtoBuf;

namespace RaceTimings.Messages;

[ProtoContract]
public record RaceBeingPlanned;

[ProtoContract]
public record RaceReady;

[ProtoContract]
public record RaceStarted;//GunFired

[ProtoContract]
public record RaceReset;

[ProtoContract]
public record RaceFinished;

[ProtoContract]
public record RaceCanceled;
