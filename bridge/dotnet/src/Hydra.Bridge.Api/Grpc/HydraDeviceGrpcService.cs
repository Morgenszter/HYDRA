using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Hydra.Bridge.Application.Devices;
using Hydra.Contracts.Devices.V1;

namespace Hydra.Bridge.Api.Grpc;

public sealed class HydraDeviceGrpcService(
    IHydraDeviceRepository repository)
    : HydraDeviceService.HydraDeviceServiceBase
{
    public override async Task<ListDevicesResponse> ListDevices(
        ListDevicesRequest request,
        ServerCallContext context)
    {
        var kinds = request.Kinds
            .Select(Map)
            .Where(kind => kind is not DeviceKindModel.Unspecified)
            .ToArray();

        var devices = await repository.ListAsync(
            kinds,
            context.CancellationToken);

        var response = new ListDevicesResponse
        {
            CapturedAt = Timestamp.FromDateTimeOffset(
                DateTimeOffset.UtcNow)
        };

        response.Devices.AddRange(devices.Select(Map));
        return response;
    }

    public override async Task StreamDeviceEvents(
        StreamDeviceEventsRequest request,
        IServerStreamWriter<DeviceEvent> responseStream,
        ServerCallContext context)
    {
        await foreach (var item in repository.StreamEventsAsync(
            request.DeviceIds,
            context.CancellationToken))
        {
            await responseStream.WriteAsync(Map(item));
        }
    }

    public override async Task<SetDeviceStateResponse> SetDeviceState(
        SetDeviceStateRequest request,
        ServerCallContext context)
    {
        var requestedAt = request.RequestedAt?.ToDateTimeOffset()
            ?? DateTimeOffset.UtcNow;

        var result = await repository.SetStateAsync(
            request.DeviceId,
            request.Capability,
            request.Value,
            requestedAt,
            context.CancellationToken);

        return new SetDeviceStateResponse
        {
            DeviceId = result.DeviceId,
            Accepted = result.Accepted,
            Message = result.Message,
            CompletedAt = Timestamp.FromDateTimeOffset(result.CompletedAt)
        };
    }

    private static DeviceDescriptor Map(DeviceDescriptorModel model)
    {
        var response = new DeviceDescriptor
        {
            DeviceId = model.DeviceId,
            DisplayName = model.DisplayName,
            Kind = Map(model.Kind),
            ConnectionState = Map(model.ConnectionState),
            UpdatedAt = Timestamp.FromDateTimeOffset(model.UpdatedAt)
        };

        response.Capabilities.AddRange(model.Capabilities);
        response.Attributes.Add(model.Attributes);

        return response;
    }

    private static DeviceEvent Map(DeviceEventModel model) => new()
    {
        EventId = model.EventId,
        DeviceId = model.DeviceId,
        ConnectionState = Map(model.ConnectionState),
        Message = model.Message,
        OccurredAt = Timestamp.FromDateTimeOffset(model.OccurredAt)
    };

    private static DeviceKindModel Map(DeviceKind value) => value switch
    {
        DeviceKind.Light => DeviceKindModel.Light,
        DeviceKind.Sensor => DeviceKindModel.Sensor,
        DeviceKind.Lock => DeviceKindModel.Lock,
        DeviceKind.Thermostat => DeviceKindModel.Thermostat,
        DeviceKind.Bridge => DeviceKindModel.Bridge,
        _ => DeviceKindModel.Unspecified
    };

    private static DeviceKind Map(DeviceKindModel value) => value switch
    {
        DeviceKindModel.Light => DeviceKind.Light,
        DeviceKindModel.Sensor => DeviceKind.Sensor,
        DeviceKindModel.Lock => DeviceKind.Lock,
        DeviceKindModel.Thermostat => DeviceKind.Thermostat,
        DeviceKindModel.Bridge => DeviceKind.Bridge,
        _ => DeviceKind.Unspecified
    };

    private static DeviceConnectionState Map(
        DeviceConnectionStateModel value) => value switch
    {
        DeviceConnectionStateModel.Online => DeviceConnectionState.Online,
        DeviceConnectionStateModel.Offline => DeviceConnectionState.Offline,
        DeviceConnectionStateModel.Degraded => DeviceConnectionState.Degraded,
        DeviceConnectionStateModel.Pairing => DeviceConnectionState.Pairing,
        _ => DeviceConnectionState.Unspecified
    };
}
