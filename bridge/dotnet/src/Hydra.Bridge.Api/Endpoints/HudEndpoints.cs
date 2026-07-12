using Hydra.Bridge.Application.AI;
using Hydra.Bridge.Application.Devices;
using Hydra.Bridge.Application.Runtime;
using Hydra.Bridge.Application.Voice;

namespace Hydra.Bridge.Api.Endpoints;

public static class HudEndpoints
{
    public static IEndpointRouteBuilder MapHydraHudEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/hud/runtime",
            async (IHydraRuntimeRepository runtimeRepository, CancellationToken cancellationToken) =>
                Results.Ok(await CreateRuntimeResponseAsync(runtimeRepository, cancellationToken)));

        endpoints.MapGet(
            "/api/hud/devices",
            async (IHydraDeviceRepository deviceRepository, CancellationToken cancellationToken) =>
            {
                var devices = await deviceRepository.ListAsync([], cancellationToken);
                return Results.Ok(new HudDevicesResponse(MapDevices(devices), DateTimeOffset.UtcNow));
            });

        endpoints.MapGet(
            "/api/hud/voice",
            async (IHydraVoiceRepository voiceRepository, CancellationToken cancellationToken) =>
            {
                var voice = await voiceRepository.GetStateAsync(cancellationToken);
                return Results.Ok(MapVoice(voice));
            });

        endpoints.MapGet(
            "/api/hud/ai-status",
            async (IModelRouter modelRouter, CancellationToken cancellationToken) =>
            {
                var status = await modelRouter.GetStatusAsync(cancellationToken);
                return Results.Ok(MapAi(status));
            });

        endpoints.MapGet(
            "/api/hud/overview",
            async (
                IHydraRuntimeRepository runtimeRepository,
                IHydraDeviceRepository deviceRepository,
                IHydraVoiceRepository voiceRepository,
                IModelRouter modelRouter,
                CancellationToken cancellationToken) =>
            {
                var runtimeTask = CreateRuntimeResponseAsync(runtimeRepository, cancellationToken);
                var devicesTask = deviceRepository.ListAsync([], cancellationToken).AsTask();
                var voiceTask = voiceRepository.GetStateAsync(cancellationToken).AsTask();
                var aiTask = modelRouter.GetStatusAsync(cancellationToken);

                await Task.WhenAll(runtimeTask, devicesTask, voiceTask, aiTask);

                return Results.Ok(
                    new HudOverviewResponse(
                        runtimeTask.Result,
                        new HudDevicesResponse(MapDevices(devicesTask.Result), DateTimeOffset.UtcNow),
                        MapVoice(voiceTask.Result),
                        MapAi(aiTask.Result),
                        DateTimeOffset.UtcNow));
            });

        return endpoints;
    }

    private static async Task<HudRuntimeResponse> CreateRuntimeResponseAsync(
        IHydraRuntimeRepository runtimeRepository,
        CancellationToken cancellationToken)
    {
        var runtime = await runtimeRepository.GetSnapshotAsync(cancellationToken);
        return new HudRuntimeResponse(
            runtime.RuntimeId,
            runtime.Health.ToString().ToUpperInvariant(),
            runtime.Subsystems.Select(
                    subsystem => new HudRuntimeSubsystemResponse(
                        subsystem.Id,
                        subsystem.DisplayName,
                        subsystem.Health.ToString().ToUpperInvariant(),
                        new Dictionary<string, string>(subsystem.Attributes)))
                .ToArray(),
            runtime.CapturedAt);
    }

    private static HudVoiceResponse MapVoice(VoiceStateModel voice) =>
        new(
            voice.Mode.ToString().ToUpperInvariant(),
            voice.ActiveProfile,
            voice.LastTranscript,
            voice.LastIntent,
            voice.UpdatedAt);

    private static HudAiStatusResponse MapAi(AiProviderStatus status) =>
        new(
            status.OllamaAvailable,
            status.OllamaModel,
            status.CloudEnabled,
            status.OllamaAvailable
                ? $"OLLAMA // {status.OllamaModel}"
                : status.CloudEnabled
                    ? "CLOUD ENABLED"
                    : "OFFLINE");

    private static HudDeviceResponse[] MapDevices(IReadOnlyList<DeviceDescriptorModel> devices) =>
        devices.Select(
                device => new HudDeviceResponse(
                    device.DeviceId,
                    device.DisplayName,
                    device.Kind.ToString().ToUpperInvariant(),
                    device.ConnectionState.ToString().ToUpperInvariant(),
                    device.Capabilities.ToArray(),
                    new Dictionary<string, string>(device.Attributes),
                    device.UpdatedAt))
            .ToArray();
}

public sealed record HudOverviewResponse(
    HudRuntimeResponse Runtime,
    HudDevicesResponse Devices,
    HudVoiceResponse Voice,
    HudAiStatusResponse Ai,
    DateTimeOffset CapturedAt);

public sealed record HudRuntimeResponse(
    string RuntimeId,
    string Health,
    IReadOnlyList<HudRuntimeSubsystemResponse> Subsystems,
    DateTimeOffset CapturedAt);

public sealed record HudRuntimeSubsystemResponse(
    string Id,
    string DisplayName,
    string Health,
    IReadOnlyDictionary<string, string> Attributes);

public sealed record HudDevicesResponse(
    IReadOnlyList<HudDeviceResponse> Devices,
    DateTimeOffset CapturedAt);

public sealed record HudDeviceResponse(
    string DeviceId,
    string DisplayName,
    string Kind,
    string ConnectionState,
    IReadOnlyList<string> Capabilities,
    IReadOnlyDictionary<string, string> Attributes,
    DateTimeOffset UpdatedAt);

public sealed record HudVoiceResponse(
    string Mode,
    string ActiveProfile,
    string LastTranscript,
    string LastIntent,
    DateTimeOffset UpdatedAt);

public sealed record HudAiStatusResponse(
    bool OllamaAvailable,
    string OllamaModel,
    bool CloudEnabled,
    string DisplayName);