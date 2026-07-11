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
        IReadOnlyList<ModuleStatus> modules;

        try
        {
            modules = await _runtimeClient.GetModulesAsync();
        }
        catch (Exception ex)
        {
            modules = CreateDegradedModules(ex.Message);
        }

        Modules.Clear();
        foreach (var module in modules)
            Modules.Add(module);

        RuntimeState = modules.Any(m => m.Name == "BRIDGE" && m.Online)
            ? "ONLINE"
            : "DEGRADED";

        AiProvider = modules.FirstOrDefault(m => m.Name == "AI")?.Detail ?? "NOT SELECTED";

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
                VoiceState = "LISTENING";
                Conversation.Add("HYDRA: Voice subsystem armed. Wake word: OMEGON.");
                break;
            case "voice off":
                VoiceState = "STANDBY";
                Conversation.Add("HYDRA: Voice subsystem suspended.");
                break;
            case "help":
                Conversation.Add("HYDRA: Available commands: status, voice on, voice off, devices, scenes, clear.");
                break;
            case "devices":
                await RefreshAsync();
                Conversation.Add("HYDRA: Smart home device bindings are pending.");
                break;
            case "scenes":
                Conversation.Add("HYDRA: Scene engine standby. No active automations.");
                break;
            case "clear":
                ClearLogs();
                break;
            default:
                Conversation.Add("HYDRA: Command queued for AI/automation routing.");
                break;
        }

        CommandText = string.Empty;
    }

    [RelayCommand]
    private void ClearLogs()
    {
        Logs.Clear();
        Logs.Add("[LOG] Buffer cleared.");
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
        CommandText = VoiceState == "LISTENING" ? "voice off" : "voice on";
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