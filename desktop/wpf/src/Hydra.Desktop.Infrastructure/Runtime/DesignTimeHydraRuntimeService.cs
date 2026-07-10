using Hydra.Desktop.Core.Runtime;

namespace Hydra.Desktop.Infrastructure.Runtime;

public sealed class DesignTimeHydraRuntimeService : IHydraRuntimeService
{
    public Task<RuntimeSnapshotModel> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new RuntimeSnapshotModel(
            "hydra-home.local",
            "READY",
            DateTimeOffset.UtcNow));
    }
}