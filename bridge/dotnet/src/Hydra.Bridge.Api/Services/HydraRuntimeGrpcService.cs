using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Hydra.Bridge.Application.Runtime;
using Hydra.Contracts.Runtime.V1;

namespace Hydra.Bridge.Api.Services;

public sealed class HydraRuntimeGrpcService(IHydraRuntimeRepository repository)
    : HydraRuntimeService.HydraRuntimeServiceBase
{
    public override async Task<RuntimeSnapshot> GetRuntimeSnapshot(
        GetRuntimeSnapshotRequest request,
        ServerCallContext context)
    {
        var snapshot = await repository.GetSnapshotAsync(request.ClientId, context.CancellationToken);

        return new RuntimeSnapshot
        {
            RuntimeId = snapshot.RuntimeId,
            Health = MapHealth(snapshot.Health),
            CapturedAt = Timestamp.FromDateTimeOffset(snapshot.CapturedAt),
            Subsystems = { snapshot.Subsystems.Select(MapSubsystem) }
        };
    }

    public override async Task StreamRuntimeEvents(
        StreamRuntimeEventsRequest request,
        IServerStreamWriter<RuntimeEvent> responseStream,
        ServerCallContext context)
    {
        var snapshot = await repository.GetSnapshotAsync(request.ClientId, context.CancellationToken);

        await responseStream.WriteAsync(new RuntimeEvent
        {
            EventId = Guid.NewGuid().ToString("N"),
            EventType = "HYDRA_RUNTIME_SNAPSHOT_READY",
            Severity = MapHealth(snapshot.Health),
            Message = "HYDRA runtime snapshot is available.",
            OccurredAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow)
        }, context.CancellationToken);
    }

    public override Task<ExecuteCommandResponse> ExecuteCommand(
        ExecuteCommandRequest request,
        ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.CommandId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "command_id is required"));
        }

        return Task.FromResult(new ExecuteCommandResponse
        {
            CommandId = request.CommandId,
            Status = CommandStatus.Accepted,
            Message = "Command accepted by HYDRA bridge stub.",
            CompletedAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow)
        });
    }

    private static RuntimeSubsystem MapSubsystem(HydraRuntimeSubsystem subsystem)
    {
        var result = new RuntimeSubsystem
        {
            Id = subsystem.Id,
            DisplayName = subsystem.DisplayName,
            Health = MapHealth(subsystem.Health)
        };

        foreach (var attribute in subsystem.Attributes)
        {
            result.Attributes.Add(attribute.Key, attribute.Value);
        }

        return result;
    }

    private static RuntimeHealth MapHealth(HydraRuntimeHealth health) => health switch
    {
        HydraRuntimeHealth.Ready => RuntimeHealth.Ready,
        HydraRuntimeHealth.Degraded => RuntimeHealth.Degraded,
        HydraRuntimeHealth.Critical => RuntimeHealth.Critical,
        HydraRuntimeHealth.Offline => RuntimeHealth.Offline,
        _ => RuntimeHealth.Unspecified
    };
}