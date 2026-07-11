using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Hydra.Bridge.Application.Runtime;
using Hydra.Contracts.Runtime.V1;

namespace Hydra.Bridge.Api.Grpc;

public sealed class HydraRuntimeGrpcService(
    IHydraRuntimeRepository repository,
    ILogger<HydraRuntimeGrpcService> logger)
    : HydraRuntimeService.HydraRuntimeServiceBase
{
    public override async Task<RuntimeSnapshot> GetRuntimeSnapshot(
        GetRuntimeSnapshotRequest request,
        ServerCallContext context)
    {
        var snapshot = await repository.GetSnapshotAsync(
            context.CancellationToken);

        return Map(snapshot);
    }

    public override async Task StreamRuntimeEvents(
        StreamRuntimeEventsRequest request,
        IServerStreamWriter<RuntimeEvent> responseStream,
        ServerCallContext context)
    {
        await foreach (var item in repository.StreamEventsAsync(
            request.EventTypes,
            context.CancellationToken))
        {
            await responseStream.WriteAsync(Map(item));
        }
    }

    public override async Task<ExecuteCommandResponse> ExecuteCommand(
        ExecuteCommandRequest request,
        ServerCallContext context)
    {
        var requestedAt = request.RequestedAt?.ToDateTimeOffset()
            ?? DateTimeOffset.UtcNow;

        var result = await repository.ExecuteCommandAsync(
            request.CommandId,
            request.Arguments,
            requestedAt,
            context.CancellationToken);

        logger.LogInformation(
            "Runtime command {CommandId} accepted={Accepted}",
            result.CommandId,
            result.Accepted);

        return new ExecuteCommandResponse
        {
            CommandId = result.CommandId,
            Status = result.Accepted
                ? CommandStatus.Completed
                : CommandStatus.Rejected,
            Message = result.Message,
            CompletedAt = Timestamp.FromDateTimeOffset(result.CompletedAt)
        };
    }

    private static RuntimeSnapshot Map(RuntimeSnapshotModel model)
    {
        var response = new RuntimeSnapshot
        {
            RuntimeId = model.RuntimeId,
            Health = Map(model.Health),
            CapturedAt = Timestamp.FromDateTimeOffset(model.CapturedAt)
        };

        response.Subsystems.AddRange(
            model.Subsystems.Select(
                subsystem =>
                {
                    var mapped = new RuntimeSubsystem
                    {
                        Id = subsystem.Id,
                        DisplayName = subsystem.DisplayName,
                        Health = Map(subsystem.Health)
                    };

                    foreach (var attribute in subsystem.Attributes)
                    {
                        mapped.Attributes.Add(attribute.Key, attribute.Value);
                    }
                    return mapped;
                }));

        return response;
    }

    private static RuntimeEvent Map(RuntimeEventModel model)
    {
        var response = new RuntimeEvent
        {
            EventId = model.EventId,
            EventType = model.EventType,
            Severity = Map(model.Severity),
            Message = model.Message,
            OccurredAt = Timestamp.FromDateTimeOffset(model.OccurredAt)
        };

        foreach (var attribute in model.Attributes)
        {
            response.Attributes.Add(attribute.Key, attribute.Value);
        }
        return response;
    }

    private static RuntimeHealth Map(RuntimeHealthState state) => state switch
    {
        RuntimeHealthState.Ready => RuntimeHealth.Ready,
        RuntimeHealthState.Degraded => RuntimeHealth.Degraded,
        RuntimeHealthState.Critical => RuntimeHealth.Critical,
        RuntimeHealthState.Offline => RuntimeHealth.Offline,
        _ => RuntimeHealth.Unspecified
    };
}
