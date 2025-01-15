namespace RaceTimings.ProtoActorServer;

internal static class SystemConfiguration
{
    internal static RedisConfiguration Redis { get; set; } = null!;
    internal static MqqtConfiguration Mqqt { get; set; } = null!;
}

public record RedisConfiguration
{
    public required string ConnectionString { get; init; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
}

public record MqqtConfiguration
{
    public required string Host { get; init; }
    public int? Port { get; init; }
    public static string DevicePathTopic(Guid deviceId) => $"/devices/{deviceId}/messages";
    public static string RaceTopic(Guid raceId) => $"/races/{raceId}/messages";
}