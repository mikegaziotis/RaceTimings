namespace RaceTimings.Messages;

public interface IRequest;
public interface IResponse;

public interface IFailureResponse: IResponse
{
    int Code { get; init; }
    string Error { get; init; }
}



