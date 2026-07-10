using Hydra.Bridge.Application.Runtime;

namespace Hydra.Bridge.Infrastructure.Runtime;

public sealed class InMemoryHydraRuntimeRepository : IHydraRuntimeRepository
{
    public ValueTask<HydraRuntimeSnapshot> GetSnapshotAsync(string clientId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        HydraRuntimeSubsystem[] subsystems =
        [
            new(
                "bridge.grpc",
                "HYDRA .NET gRPC Bridge",
                HydraRuntimeHealth.Ready,
                new Dictionary<string, string>
                {
                    ["clientId"] = clientId,
                    ["transport"] = "grpc"
                }),
            new(
                "core.rust",
                "HYDRA Rust Core",
                HydraRuntimeHealth.Degraded,
                new Dictionary<string, string>
                {
                    ["status"] = "stub-awaiting-tonic-integration"
                })
        ];

        var snapshot = new HydraRuntimeSnapshot(
            RuntimeId: "hydra-home.local",
            Health: HydraRuntimeHealth.Ready,
            Subsystems: subsystems,
            CapturedAt: DateTimeOffset.UtcNow);

        return ValueTask.FromResult(snapshot);
    }
}