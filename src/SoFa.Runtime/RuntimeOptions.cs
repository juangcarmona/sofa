namespace SoFa.Runtime;

public sealed record RuntimeOptions
{
    public required string Socket { get; init; }
    public required uint ClientUid { get; init; }
    public required string PolicyFile { get; init; }
    public required string ContractRoot { get; init; }
    public required string WorkRoot { get; init; }
    public required string GithubTokenFile { get; init; }
    public required string EngineImage { get; init; }
    public required string EngineAuthVolume { get; init; }
    public required string Model { get; init; }
    public uint EngineUid { get; init; } = 20003;
}

public class Rejected(string message) : Exception(message);
