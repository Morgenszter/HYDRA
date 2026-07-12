using System.Net.Http;
using System.Net.Http.Json;
using Google.Protobuf.WellKnownTypes;
using Hydra.Contracts.AI.V1;
using Hydra.Contracts.Devices.V1;
using Hydra.Contracts.Runtime.V1;
using Hydra.Contracts.Voice.V1;

namespace Hydra.Desktop.Hud.Services;

public sealed record ModuleStatus(string Name, string State, string Detail, bool Online);

public sealed record VoiceIntentSubmissionResult(string Intent, bool Accepted, string Message);

public sealed record RuntimeCommandExecutionResult(
    string CommandId,
    string Status,
    bool Accepted,
    string Message);

public sealed record HudOverview(
    string RuntimeState,
    string VoiceState,
    string AiProvider,
    string WakeWord,
    string SmartHomeSummary,
    string LastTranscript,
    string LastIntent,
    IReadOnlyList<ModuleStatus> Modules,
    IReadOnlyList<string> DeviceLines);

public sealed class HydraRuntimeClient(
    HttpClient httpClient,
    HydraRuntimeService.HydraRuntimeServiceClient runtimeClient,
    HydraDeviceService.HydraDeviceServiceClient deviceClient,
    HydraVoiceService.HydraVoiceServiceClient voiceClient,
    HydraAiService.HydraAiServiceClient aiClient)
{
    public async Task<HudOverview> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var runtimeTask = runtimeClient.GetRuntimeSnapshotAsync(
                new GetRuntimeSnapshotRequest { ClientId = "hydra-hud" },
                cancellationToken: cancellationToken).ResponseAsync;
            var devicesTask = deviceClient.ListDevicesAsync(
                new ListDevicesRequest { ClientId = "hydra-hud" },
                cancellationToken: cancellationToken).ResponseAsync;
            var voiceTask = voiceClient.GetVoiceStateAsync(
                new GetVoiceStateRequest { ClientId = "hydra-hud" },
                cancellationToken: cancellationToken).ResponseAsync;
            var aiTask = aiClient.GetProviderStatusAsync(
                new GetProviderStatusRequest(),
                cancellationToken: cancellationToken).ResponseAsync;

            await Task.WhenAll(runtimeTask, devicesTask, voiceTask, aiTask);
            return MapOverview(runtimeTask.Result, devicesTask.Result, voiceTask.Result, aiTask.Result);
        }
        catch (Exception grpcException)
        {
            try
            {
                var overview = await httpClient.GetFromJsonAsync<RestHudOverview>("/api/hud/overview", cancellationToken);
                if (overview is not null)
                {
                    return MapRestOverview(overview);
                }
            }
            catch
            {
            }

            return CreateFallbackOverview(grpcException.Message);
        }
    }

    public async Task<VoiceIntentSubmissionResult> SubmitTranscriptAsync(
        string transcript,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await voiceClient.SubmitTranscriptAsync(
                new SubmitTranscriptRequest
                {
                    Transcript = transcript,
                    Locale = "pl-PL"
                },
                cancellationToken: cancellationToken).ResponseAsync;

            return new VoiceIntentSubmissionResult(response.Intent, response.Accepted, response.Reason);
        }
        catch (Exception ex)
        {
            return new VoiceIntentSubmissionResult("UNAVAILABLE", false, ex.Message);
        }
    }

    public async Task<RuntimeCommandExecutionResult> ExecuteRuntimeCommandAsync(
        string command,
        CancellationToken cancellationToken = default)
    {
        var trimmed = command.Trim();
        if (trimmed.Length == 0)
        {
            return new RuntimeCommandExecutionResult(
                string.Empty,
                "REJECTED",
                false,
                "Command is empty.");
        }

        var commandId = CreateCommandId(trimmed);

        try
        {
            var response = await runtimeClient.ExecuteCommandAsync(
                new ExecuteCommandRequest
                {
                    CommandId = commandId,
                    RequestedAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow)
                },
                cancellationToken: cancellationToken).ResponseAsync;

            return new RuntimeCommandExecutionResult(
                response.CommandId,
                response.Status.ToString().ToUpperInvariant(),
                response.Status is CommandStatus.Accepted or CommandStatus.Completed,
                response.Message);
        }
        catch (Exception ex)
        {
            return new RuntimeCommandExecutionResult(
                commandId,
                "FAILED",
                false,
                ex.Message);
        }
    }

    private static HudOverview MapOverview(
        RuntimeSnapshot runtime,
        ListDevicesResponse devices,
        VoiceState voice,
        GetProviderStatusResponse ai)
    {
        var deviceLines = devices.Devices
            .Select(device => $"{device.DisplayName} :: {MapDeviceState(device.ConnectionState)} :: {string.Join(", ", device.Capabilities)}")
            .ToArray();

        var onlineCount = devices.Devices.Count(device => device.ConnectionState == DeviceConnectionState.Online);
        var aiProvider = ai.OllamaAvailable
            ? $"OLLAMA // {ai.OllamaModel}"
            : ai.CloudEnabled
                ? "CLOUD ENABLED"
                : "OFFLINE";

        var modules = runtime.Subsystems
            .Select(
                subsystem => new ModuleStatus(
                    subsystem.DisplayName.ToUpperInvariant(),
                    MapRuntimeHealth(subsystem.Health),
                    subsystem.Attributes.Count == 0
                        ? subsystem.Id
                        : string.Join(" | ", subsystem.Attributes.Select(pair => $"{pair.Key}={pair.Value}")),
                    subsystem.Health is RuntimeHealth.Ready or RuntimeHealth.Degraded))
            .ToList();

        modules.Add(
            new ModuleStatus(
                "VOICE",
                MapVoiceMode(voice.Mode),
                string.IsNullOrWhiteSpace(voice.LastIntent)
                    ? voice.ActiveProfile
                    : $"{voice.ActiveProfile} | intent={voice.LastIntent}",
                voice.Mode is VoiceMode.Passive or VoiceMode.Armed or VoiceMode.Processing));

        modules.Add(
            new ModuleStatus(
                "SMART HOME",
                onlineCount > 0 ? "ONLINE" : "STANDBY",
                $"{onlineCount}/{devices.Devices.Count} devices online",
                onlineCount > 0));

        return new HudOverview(
            MapRuntimeHealth(runtime.Health),
            MapVoiceMode(voice.Mode),
            aiProvider,
            string.IsNullOrWhiteSpace(voice.ActiveProfile) ? "OMEGON" : voice.ActiveProfile,
            $"{onlineCount}/{devices.Devices.Count} devices online",
            voice.LastTranscript,
            voice.LastIntent,
            modules,
            deviceLines);
    }

    private static HudOverview MapRestOverview(RestHudOverview overview)
    {
        var deviceLines = overview.Devices.Devices
            .Select(device => $"{device.DisplayName} :: {device.ConnectionState} :: {string.Join(", ", device.Capabilities)}")
            .ToArray();

        var onlineCount = overview.Devices.Devices.Count(
            device => string.Equals(device.ConnectionState, "ONLINE", StringComparison.OrdinalIgnoreCase));

        var modules = overview.Runtime.Subsystems
            .Select(
                subsystem => new ModuleStatus(
                    subsystem.DisplayName.ToUpperInvariant(),
                    subsystem.Health,
                    subsystem.Attributes.Count == 0
                        ? subsystem.Id
                        : string.Join(" | ", subsystem.Attributes.Select(pair => $"{pair.Key}={pair.Value}")),
                    subsystem.Health is "READY" or "DEGRADED"))
            .ToList();

        modules.Add(
            new ModuleStatus(
                "VOICE",
                overview.Voice.Mode,
                string.IsNullOrWhiteSpace(overview.Voice.LastIntent)
                    ? overview.Voice.ActiveProfile
                    : $"{overview.Voice.ActiveProfile} | intent={overview.Voice.LastIntent}",
                overview.Voice.Mode is "PASSIVE" or "ARMED" or "PROCESSING"));

        modules.Add(
            new ModuleStatus(
                "SMART HOME",
                onlineCount > 0 ? "ONLINE" : "STANDBY",
                $"{onlineCount}/{overview.Devices.Devices.Count} devices online",
                onlineCount > 0));

        return new HudOverview(
            overview.Runtime.Health,
            overview.Voice.Mode,
            overview.Ai.DisplayName,
            string.IsNullOrWhiteSpace(overview.Voice.ActiveProfile) ? "OMEGON" : overview.Voice.ActiveProfile,
            $"{onlineCount}/{overview.Devices.Devices.Count} devices online",
            overview.Voice.LastTranscript,
            overview.Voice.LastIntent,
            modules,
            deviceLines);
    }

    private static HudOverview CreateFallbackOverview(string reason) =>
        new(
            "DEGRADED",
            "OFFLINE",
            "UNAVAILABLE",
            "OMEGON",
            "0/0 devices online",
            string.Empty,
            string.Empty,
            [
                new ModuleStatus("BRIDGE", "OFFLINE", reason, false),
                new ModuleStatus("VOICE", "OFFLINE", "Bridge voice service unavailable", false),
                new ModuleStatus("SMART HOME", "STANDBY", "No telemetry available", false)
            ],
            []);

    private static string CreateCommandId(string command)
    {
        var slug = new string(
            command
                .ToLowerInvariant()
                .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
                .ToArray())
            .Trim('-');

        if (slug.Length == 0)
        {
            slug = "runtime-command";
        }

        return $"hud.{slug}";
    }

    private static string MapRuntimeHealth(RuntimeHealth health) => health switch
    {
        RuntimeHealth.Ready => "ONLINE",
        RuntimeHealth.Degraded => "DEGRADED",
        RuntimeHealth.Critical => "CRITICAL",
        RuntimeHealth.Offline => "OFFLINE",
        _ => "UNKNOWN"
    };

    private static string MapVoiceMode(VoiceMode mode) => mode switch
    {
        VoiceMode.Disabled => "DISABLED",
        VoiceMode.Passive => "PASSIVE",
        VoiceMode.Armed => "ARMED",
        VoiceMode.Processing => "PROCESSING",
        _ => "UNSPECIFIED"
    };

    private static string MapDeviceState(DeviceConnectionState state) => state switch
    {
        DeviceConnectionState.Online => "ONLINE",
        DeviceConnectionState.Offline => "OFFLINE",
        DeviceConnectionState.Degraded => "DEGRADED",
        DeviceConnectionState.Pairing => "PAIRING",
        _ => "UNKNOWN"
    };

    private sealed record RestHudOverview(
        RestHudRuntime Runtime,
        RestHudDevices Devices,
        RestHudVoice Voice,
        RestHudAi Ai,
        DateTimeOffset CapturedAt);

    private sealed record RestHudRuntime(
        string RuntimeId,
        string Health,
        IReadOnlyList<RestHudSubsystem> Subsystems,
        DateTimeOffset CapturedAt);

    private sealed record RestHudSubsystem(
        string Id,
        string DisplayName,
        string Health,
        IReadOnlyDictionary<string, string> Attributes);

    private sealed record RestHudDevices(
        IReadOnlyList<RestHudDevice> Devices,
        DateTimeOffset CapturedAt);

    private sealed record RestHudDevice(
        string DeviceId,
        string DisplayName,
        string Kind,
        string ConnectionState,
        IReadOnlyList<string> Capabilities,
        IReadOnlyDictionary<string, string> Attributes,
        DateTimeOffset UpdatedAt);

    private sealed record RestHudVoice(
        string Mode,
        string ActiveProfile,
        string LastTranscript,
        string LastIntent,
        DateTimeOffset UpdatedAt);

    private sealed record RestHudAi(
        bool OllamaAvailable,
        string OllamaModel,
        bool CloudEnabled,
        string DisplayName);
}