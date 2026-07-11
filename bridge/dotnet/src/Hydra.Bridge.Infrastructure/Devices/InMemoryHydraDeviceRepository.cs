using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Hydra.Bridge.Application.Devices;

namespace Hydra.Bridge.Infrastructure.Devices;

public sealed class InMemoryHydraDeviceRepository : IHydraDeviceRepository
{
    private readonly ConcurrentDictionary<string, DeviceDescriptorModel> _devices =
        new(
        [
            new KeyValuePair<string, DeviceDescriptorModel>(
                "heater.main",
                new DeviceDescriptorModel(
                    "heater.main",
                    "GRZEJNIK",
                    DeviceKindModel.Thermostat,
                    DeviceConnectionStateModel.Online,
                    ["power", "temperature", "mode"],
                    new Dictionary<string, string>
                    {
                        ["temperature"] = "22",
                        ["mode"] = "AUTO",
                        ["power"] = "ON"
                    },
                    DateTimeOffset.UtcNow)),
            new KeyValuePair<string, DeviceDescriptorModel>(
                "light.led.strip",
                new DeviceDescriptorModel(
                    "light.led.strip",
                    "Listwa LED",
                    DeviceKindModel.Light,
                    DeviceConnectionStateModel.Online,
                    ["power", "brightness", "color"],
                    new Dictionary<string, string>
                    {
                        ["power"] = "ON",
                        ["brightness"] = "80",
                        ["color"] = "#00FFD5"
                    },
                    DateTimeOffset.UtcNow)),
            new KeyValuePair<string, DeviceDescriptorModel>(
                "light.tapo.1",
                new DeviceDescriptorModel(
                    "light.tapo.1",
                    "Żarówka Tapo 1",
                    DeviceKindModel.Light,
                    DeviceConnectionStateModel.Online,
                    ["power", "brightness", "color_temperature"],
                    new Dictionary<string, string>
                    {
                        ["power"] = "ON",
                        ["brightness"] = "60",
                        ["color_temperature"] = "4000"
                    },
                    DateTimeOffset.UtcNow))
        ]);

    public ValueTask<IReadOnlyList<DeviceDescriptorModel>> ListAsync(
        IReadOnlyCollection<DeviceKindModel> kinds,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var items = _devices.Values
            .Where(device => kinds.Count == 0 || kinds.Contains(device.Kind))
            .OrderBy(device => device.DisplayName)
            .ToArray();

        return ValueTask.FromResult<IReadOnlyList<DeviceDescriptorModel>>(items);
    }

    public async IAsyncEnumerable<DeviceEventModel> StreamEventsAsync(
        IReadOnlyCollection<string> deviceIds,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

            foreach (var device in _devices.Values)
            {
                if (deviceIds.Count > 0 && !deviceIds.Contains(device.DeviceId))
                {
                    continue;
                }

                yield return new DeviceEventModel(
                    Guid.NewGuid().ToString("N"),
                    device.DeviceId,
                    device.ConnectionState,
                    $"{device.DisplayName} state is {device.ConnectionState}.",
                    DateTimeOffset.UtcNow);
            }
        }
    }

    public ValueTask<SetDeviceStateResult> SetStateAsync(
        string deviceId,
        string capability,
        string value,
        DateTimeOffset requestedAt,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_devices.TryGetValue(deviceId, out var current))
        {
            return ValueTask.FromResult(
                new SetDeviceStateResult(
                    deviceId,
                    false,
                    "Device not found.",
                    DateTimeOffset.UtcNow));
        }

        if (!current.Capabilities.Contains(capability, StringComparer.OrdinalIgnoreCase))
        {
            return ValueTask.FromResult(
                new SetDeviceStateResult(
                    deviceId,
                    false,
                    $"Capability '{capability}' is not supported.",
                    DateTimeOffset.UtcNow));
        }

        var attributes = new Dictionary<string, string>(current.Attributes)
        {
            [capability] = value
        };

        _devices[deviceId] = current with
        {
            Attributes = attributes,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        return ValueTask.FromResult(
            new SetDeviceStateResult(
                deviceId,
                true,
                $"{capability} updated to '{value}'.",
                DateTimeOffset.UtcNow));
    }
}
