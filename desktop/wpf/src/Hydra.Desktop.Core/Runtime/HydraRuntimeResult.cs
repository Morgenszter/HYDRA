namespace Hydra.Desktop.Core.Runtime;

public sealed record HydraRuntimeResult(
    bool IsSuccess,
    RuntimeSnapshotModel? Snapshot,
    string? ErrorMessage)
{
    public static HydraRuntimeResult Success(RuntimeSnapshotModel snapshot) =>
        new(true, snapshot, null);

    public static HydraRuntimeResult Failure(string errorMessage) =>
        new(false, null, errorMessage);
}