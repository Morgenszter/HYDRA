namespace Hydra.Desktop.Core.Runtime;

public interface IHydraBridgeLifecycleService
{
    Task<HydraBridgeLifecycleResult> EnsureBridgeAsync(CancellationToken cancellationToken);
}