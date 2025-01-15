namespace RaceTimings.ProtoActorServer.Providers;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}

public class DateTimeProvider: IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}