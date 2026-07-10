namespace Hydra.Bridge.Application.Devices;

public enum DeviceKindModel
{
    Unspecified,
    Light,
    Sensor,
    Lock,
    Thermostat,
    Bridge
}

public enum DeviceConnectionStateModel
{
    Unspecified,
    Online,
    Offline,
    Degraded,
    Pairing
}

public sealed record DeviceDescriptorModel(
    string DeviceId,
    string DisplayName,
    DeviceKindModel Kind,
    DeviceConnectionStateModel ConnectionState,
    IReadOnlyList<string> Capabilities,
    IReadOnlyDictionary<string, string> Attributes,
    DateTimeOffset UpdatedAt);

public sealed record DeviceEventModel(
    string EventId,
    string DeviceId,
    DeviceConnectionStateModel ConnectionState,
    string Message,
    DateTimeOffset OccurredAt);

public sealed record SetDeviceStateResult(
    string DeviceId,
    bool Accepted,
    string Message,
    DateTimeOffset CompletedAt);
