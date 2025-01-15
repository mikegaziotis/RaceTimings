using ProtoBuf;

namespace RaceTimings.Messages;

[ProtoContract]
public record Country
{
    [ProtoMember(1)]
    public required string Id { get; init; }

    [ProtoIgnore]
    public string ThreeLetterCode => Id; 
    
    [ProtoMember(2)]
    public required string FullName { get; init; }
    
    [ProtoMember(3)]
    public int Colour { get; init; }
};