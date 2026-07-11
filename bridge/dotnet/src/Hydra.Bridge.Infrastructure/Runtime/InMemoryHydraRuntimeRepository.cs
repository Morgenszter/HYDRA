using System.Runtime.CompilerServices;
using Hydra.Bridge.Application.Runtime;

namespace Hydra.Bridge.Infrastructure.Runtime;

public sealed class InMemoryHydraRuntimeRepository : IHydraRuntimeRepository
{
    public ValueTask<RuntimeSnapshotModel> GetSnapshotAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        RuntimeSubsystemModel[] subsystems =
        [
            new(
                "bridge.dotnet",
                ".NET gRPC Bridge",
                RuntimeHealthState.Ready,
                new Dictionary<string, string>
                {
                    ["runtime"] = ".NET 8",
                    ["transport"] = "gRPC"
                }),
            new(
                "core.rust",
                "Rust Core",
                RuntimeHealthState.Degraded,
                new Dictionary<string, string>
                {
                    ["status"] = "stub-awaiting-tonic-integration"
                }),
            new(
                "ai.ollama",
                "Ollama AI",
                RuntimeHealthState.Ready,
                new Dictionary<string, string>
                {
                    ["endpoint"] = "http://127.0.0.1:11434"
                })
        ];

        return ValueTask.FromResult(
            new RuntimeSnapshotModel(
                "hydra-runtime-local",
                RuntimeHealthState.Degraded,
                subsystems,
                DateTimeOffset.UtcNow));
    }

    public async IAsyncEnumerable<RuntimeEventModel> StreamEventsAsync(
        IReadOnlyCollection<string> eventTypes,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            yield return new RuntimeEventModel(
                Guid.NewGuid().ToString("N"),
                "heartbeat",
                RuntimeHealthState.Ready,
                "HYDRA runtime heartbeat.",
                new Dictionary<string, string>(),
                DateTimeOffset.UtcNow);
        }
    }

    public ValueTask<RuntimeCommandResult> ExecuteCommandAsync(
        string commandId,
        IReadOnlyDictionary<string, string> arguments,
        DateTimeOffset requestedAt,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(commandId))
        {
            return ValueTask.FromResult(
                new RuntimeCommandResult(
                    commandId,
                    false,
                    "Command ID is required.",
                    DateTimeOffset.UtcNow));
        }

        return ValueTask.FromResult(
            new RuntimeCommandResult(
                commandId,
                true,
                "Command accepted by in-memory runtime.",
                DateTimeOffset.UtcNow));
    }
}
