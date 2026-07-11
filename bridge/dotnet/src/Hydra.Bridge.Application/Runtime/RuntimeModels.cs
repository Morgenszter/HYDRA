namespace Hydra.Bridge.Application.Runtime;

public enum RuntimeHealthState
{
    Unspecified,
    Ready,
    Degraded,
    Critical,
    Offline
}

public sealed record RuntimeSubsystemModel(
    string Id,
    string DisplayName,
    RuntimeHealthState Health,
    IReadOnlyDictionary<string, string> Attributes);

public sealed record RuntimeSnapshotModel(
    string RuntimeId,
    RuntimeHealthState Health,
    IReadOnlyList<RuntimeSubsystemModel> Subsystems,
    DateTimeOffset CapturedAt);

public sealed record RuntimeEventModel(
    string EventId,
    string EventType,
    RuntimeHealthState Severity,
    string Message,
    IReadOnlyDictionary<string, string> Attributes,
    DateTimeOffset OccurredAt);

public sealed record RuntimeCommandResult(
    string CommandId,
    bool Accepted,
    string Message,
    DateTimeOffset CompletedAt);
