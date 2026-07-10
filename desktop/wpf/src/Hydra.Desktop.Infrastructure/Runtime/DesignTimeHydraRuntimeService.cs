using Hydra.Desktop.Core.Runtime;

namespace Hydra.Desktop.Infrastructure.Runtime;

public sealed class DesignTimeHydraRuntimeService : IHydraRuntimeService
{
    public Task<HydraRuntimeResult> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var snapshot = new RuntimeSnapshotModel(
            "hydra-home.local",
            "READY",
            DateTimeOffset.UtcNow);

        return Task.FromResult(HydraRuntimeResult.Success(snapshot));
    }
}