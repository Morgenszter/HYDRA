namespace Hydra.Bridge.Application.Runtime;

public interface IHydraRuntimeRepository
{
    ValueTask<RuntimeSnapshotModel> GetSnapshotAsync(
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<RuntimeEventModel> StreamEventsAsync(
        IReadOnlyCollection<string> eventTypes,
        CancellationToken cancellationToken = default);

    ValueTask<RuntimeCommandResult> ExecuteCommandAsync(
        string commandId,
        IReadOnlyDictionary<string, string> arguments,
        DateTimeOffset requestedAt,
        CancellationToken cancellationToken = default);
}
