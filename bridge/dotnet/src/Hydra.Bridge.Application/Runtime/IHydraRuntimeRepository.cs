namespace Hydra.Bridge.Application.Runtime;

public interface IHydraRuntimeRepository
{
    ValueTask<HydraRuntimeSnapshot> GetSnapshotAsync(string clientId, CancellationToken cancellationToken);
}
