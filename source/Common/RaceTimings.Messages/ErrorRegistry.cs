using System.Collections.Immutable;
using System.Net;

namespace RaceTimings.Messages;


public enum ErrorCode
{
    //Common
    
    InvalidLaneNumber = 101,
    LaneAlreadyTaken = 102,
    //RaceAthleteRelated
    AthleteAlreadyExists = 103,
    AthleteNotInRace = 104,
    CannotSwapAthleteToSameLane = 105,
    CannotSwapEmptyLanes = 106,
    //AthleteRelated
    AthleteInvalidName = 107,
    AthleteCantChangeSex = 108,
    AthleteInvalidSex = 109,
    AthleteInvalidAge = 110,
    AthleteInvalidCountry = 111,
    //DeviceRelated
    DeviceNotFound = 201,
    DeviceStoreAccessError = 202,
    DeviceAlreadyExists = 203,
    //Country Related
    
    
    UnknownError = 500,
}

public static class ErrorRegistry
{
    private static readonly ImmutableDictionary<ErrorCode, string> ErrorMessages = 
        ImmutableDictionary.CreateRange([
            new KeyValuePair<ErrorCode, string>(ErrorCode.InvalidLaneNumber, "Invalid lane number. Lane number out of bounds"),
            new KeyValuePair<ErrorCode, string>(ErrorCode.LaneAlreadyTaken, "This lane already taken. You cannot add two athletes to the same lane"),
            new KeyValuePair<ErrorCode, string>(ErrorCode.AthleteAlreadyExists, "Athlete is already assigned to the race exists. You cannot add the same athlete twice"),
            new KeyValuePair<ErrorCode, string>(ErrorCode.AthleteNotInRace, "Cannot remove an athlete currently not assigned to the race"),
            new KeyValuePair<ErrorCode, string>(ErrorCode.CannotSwapAthleteToSameLane, "Athlete is already in assigned lane. You cannot swap athlete to the same lane as they already are."),
            new KeyValuePair<ErrorCode, string>(ErrorCode.CannotSwapEmptyLanes, "There are no athletes on either lane. You cannot swap empty lanes."),
            new KeyValuePair<ErrorCode, string>(ErrorCode.UnknownError, "An unknown error occurred")
        ]);

    private static readonly ImmutableDictionary<ErrorCode, HttpStatusCode> HttpErrorMappings =
        ImmutableDictionary.CreateRange([
            new KeyValuePair<ErrorCode, HttpStatusCode>(ErrorCode.InvalidLaneNumber, HttpStatusCode.BadRequest),
        ]);
        

    public static string Get(ErrorCode code) => CollectionExtensions.GetValueOrDefault(ErrorMessages, code, "An unknown error occurred");

}