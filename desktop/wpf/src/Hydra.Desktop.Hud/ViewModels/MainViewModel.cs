using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hydra.Desktop.Hud.Services;

namespace Hydra.Desktop.Hud.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly HydraRuntimeClient _runtimeClient;
    private readonly System.Threading.Timer _clockTimer;

    public ObservableCollection<ModuleStatus> Modules { get; } = [];
    public ObservableCollection<string> Logs { get; } = [];
    public ObservableCollection<string> Conversation { get; } = [];
    public ObservableCollection<string> DeviceLines { get; } = [];

    [ObservableProperty]
    private string commandText = string.Empty;

    [ObservableProperty]
    private string runtimeState = "INITIALIZING";

    [ObservableProperty]
    private string voiceState = "STANDBY";

    [ObservableProperty]
    private string aiProvider = "NOT SELECTED";

    [ObservableProperty]
    private string currentTime = DateTime.Now.ToString("HH:mm:ss");

    [ObservableProperty]
    private string wakeWord = "OMEGON";

    [ObservableProperty]
    private string smartHomeSummary = "0/0 devices online";

    [ObservableProperty]
    private string lastTranscript = string.Empty;

    [ObservableProperty]
    private string lastIntent = string.Empty;

    public MainViewModel(HydraRuntimeClient runtimeClient)
    {
        _runtimeClient = runtimeClient;
        _clockTimer = new System.Threading.Timer(
            _ => OnClockTick(),
            null,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1));

        Logs.Add("[BOOT] HYDRA HUD initialized.");
        Logs.Add("[BOOT] Awaiting runtime telemetry.");
        Conversation.Add("HYDRA: Command interface online.");
    }

    private void OnClockTick()
    {
        CurrentTime = DateTime.Now.ToString("HH:mm:ss");
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        RuntimeState = "SCANNING";

        try
        {
            var overview = await _runtimeClient.GetOverviewAsync();

            Modules.Clear();
            foreach (var module in overview.Modules)
                Modules.Add(module);

            DeviceLines.Clear();
            foreach (var deviceLine in overview.DeviceLines)
                DeviceLines.Add(deviceLine);

            RuntimeState = overview.RuntimeState;
            VoiceState = overview.VoiceState;
            AiProvider = overview.AiProvider;
            WakeWord = overview.WakeWord;
            SmartHomeSummary = overview.SmartHomeSummary;
            LastTranscript = overview.LastTranscript;
            LastIntent = overview.LastIntent;
        }
        catch (Exception ex)
        {
            Modules.Clear();
            foreach (var module in CreateDegradedModules(ex.Message))
                Modules.Add(module);
            DeviceLines.Clear();
        }

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        Logs.Insert(0, $"[{timestamp}] Runtime scan complete: {RuntimeState}.");
    }

    [RelayCommand]
    private async Task ExecuteCommandAsync()
    {
        var command = CommandText.Trim();
        if (command.Length == 0)
            return;

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        Conversation.Add($"YOU: {command}");
        Logs.Insert(0, $"[{timestamp}] COMMAND: {command}");

        switch (command.ToLowerInvariant())
        {
            case "status":
                await RefreshAsync();
                Conversation.Add($"HYDRA: Runtime state is {RuntimeState}.");
                break;
            case "voice on":
                VoiceState = "ARMED";
                Conversation.Add($"HYDRA: Voice subsystem armed. Wake word: {WakeWord}.");
                break;
            case "voice off":
                VoiceState = "PASSIVE";
                Conversation.Add("HYDRA: Voice subsystem suspended.");
                break;
            case "help":
                Conversation.Add("HYDRA: Available commands: status, voice on, voice off, devices, scenes, clear.");
                break;
            case "devices":
                await RefreshAsync();
                Conversation.Add(
                    DeviceLines.Count == 0
                        ? "HYDRA: No devices reported by bridge."
                        : $"HYDRA: Devices online -> {string.Join(" | ", DeviceLines.Take(3))}");
                break;
            case "scenes":
                Conversation.Add("HYDRA: Scene engine standby. No active automations.");
                break;
            case "clear":
                ClearLogs();
                break;
            default:
                var runtimeResult = await _runtimeClient.ExecuteRuntimeCommandAsync(command);
                Conversation.Add(
                    runtimeResult.Accepted
                        ? $"HYDRA: Runtime command {runtimeResult.CommandId} => {runtimeResult.Status} :: {runtimeResult.Message}"
                        : $"HYDRA: Runtime command failed => {runtimeResult.Status} :: {runtimeResult.Message}");

                if (!runtimeResult.Accepted)
                {
                    var intentResult = await _runtimeClient.SubmitTranscriptAsync(command);
                    Conversation.Add(
                        intentResult.Accepted
                            ? $"HYDRA: Intent={intentResult.Intent} :: {intentResult.Message}"
                            : $"HYDRA: Transcript rejected :: {intentResult.Message}");
                }

                await RefreshAsync();
                break;
        }

        CommandText = string.Empty;
    }

    [RelayCommand]
    private void ClearLogs()
    {
        Logs.Clear();
        Logs.Add("[LOG] Buffer cleared.");
        Conversation.Clear();
        Conversation.Add("HYDRA: Command interface online.");
    }

    [RelayCommand]
    private async Task WheelDevicesAsync()
    {
        CommandText = "devices";
        await ExecuteCommandAsync();
    }

    [RelayCommand]
    private async Task WheelVoiceAsync()
    {
        CommandText = VoiceState == "ARMED" ? "voice off" : "voice on";
        await ExecuteCommandAsync();
    }

    [RelayCommand]
    private async Task WheelAiAsync()
    {
        Conversation.Add("HYDRA: AI channel selected. Type your query below.");
    }

    [RelayCommand]
    private async Task WheelScenesAsync()
    {
        CommandText = "scenes";
        await ExecuteCommandAsync();
    }

    private static IReadOnlyList<ModuleStatus> CreateDegradedModules(string reason) =>
        new[]
        {
            new ModuleStatus("BRIDGE", "OFFLINE", reason, false),
            new ModuleStatus("VOICE", "READY", "STT / TTS / INTENT", true),
            new ModuleStatus("CORE", "READY", "RUNTIME ORCHESTRATOR", true),
            new ModuleStatus("AI", "STANDBY", "PROVIDER NOT SELECTED", false),
            new ModuleStatus("SMART HOME", "STANDBY", "MQTT / TUYA / TAPO / BLE", false)
        };
}