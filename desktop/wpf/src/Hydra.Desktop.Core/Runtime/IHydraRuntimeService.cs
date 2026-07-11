namespace Hydra.Desktop.Core.Runtime;

public interface IHydraRuntimeService
{
    Task<HydraRuntimeResult> GetSnapshotAsync(CancellationToken cancellationToken);
}