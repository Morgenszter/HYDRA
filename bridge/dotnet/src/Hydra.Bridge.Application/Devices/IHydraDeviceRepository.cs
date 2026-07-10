namespace Hydra.Bridge.Application.Devices;

public interface IHydraDeviceRepository
{
    ValueTask<IReadOnlyList<DeviceDescriptorModel>> ListAsync(
        IReadOnlyCollection<DeviceKindModel> kinds,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<DeviceEventModel> StreamEventsAsync(
        IReadOnlyCollection<string> deviceIds,
        CancellationToken cancellationToken = default);

    ValueTask<SetDeviceStateResult> SetStateAsync(
        string deviceId,
        string capability,
        string value,
        DateTimeOffset requestedAt,
        CancellationToken cancellationToken = default);
}
