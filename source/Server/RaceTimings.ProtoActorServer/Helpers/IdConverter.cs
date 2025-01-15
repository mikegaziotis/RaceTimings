using CSharpFunctionalExtensions;

namespace RaceTimings.ProtoActorServer.Helpers;

public static class IdConverter
{
    public static Maybe<Guid> TryGetGuidIdFromString(string keyPart) => Guid.TryParse(keyPart, out var id) ? id : Maybe<Guid>.None;
    public static Maybe<int> TryGetIntIdFromString(string keyPart) => int.TryParse(keyPart, out var id) ? id : Maybe<int>.None;
    public static Maybe<long> TryGetLongIdFromString(string keyPart) => long.TryParse(keyPart, out var id) ? id : Maybe<long>.None;
    public static Maybe<string> TryGetStringIdFromString(string keyPart) => string.IsNullOrEmpty(keyPart) ? Maybe<string>.None : keyPart;
}