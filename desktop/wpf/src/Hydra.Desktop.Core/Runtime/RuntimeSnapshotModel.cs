namespace Hydra.Desktop.Core.Runtime;

public sealed record RuntimeSnapshotModel(
    string RuntimeId,
    string Health,
    DateTimeOffset CapturedAt);