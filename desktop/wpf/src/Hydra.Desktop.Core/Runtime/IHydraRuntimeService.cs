namespace Hydra.Desktop.Core.Runtime;

public interface IHydraRuntimeService
{
    Task<RuntimeSnapshotModel> GetSnapshotAsync(CancellationToken cancellationToken);
}