namespace Hydra.Bridge.Application.Runtime;

public enum HydraRuntimeHealth
{
    Ready,
    Degraded,
    Critical,
    Offline
}

public sealed record HydraRuntimeSubsystem(
    string Id,
    string DisplayName,
    HydraRuntimeHealth Health,
    IReadOnlyDictionary<string, string> Attributes);

public sealed record HydraRuntimeSnapshot(
    string RuntimeId,
    HydraRuntimeHealth Health,
    IReadOnlyList<HydraRuntimeSubsystem> Subsystems,
    DateTimeOffset CapturedAt);
