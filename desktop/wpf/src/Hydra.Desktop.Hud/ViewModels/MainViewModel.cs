using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hydra.Desktop.Hud.Services;

namespace Hydra.Desktop.Hud.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly HydraRuntimeClient _runtimeClient;

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
        Logs.Add("[BOOT] HYDRA HUD initialized.");
        Logs.Add("[BOOT] Awaiting runtime telemetry.");
        Conversation.Add("HYDRA: Command interface online.");
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        RuntimeState = "SCANNING";
        var modules = await _runtimeClient.GetModulesAsync();

        Modules.Clear();
        foreach (var module in modules)
            Modules.Add(module);

        RuntimeState = modules.Any(m => m.Name == "BRIDGE" && m.Online)
            ? "ONLINE"
            : "DEGRADED";

        CurrentTime = DateTime.Now.ToString("HH:mm:ss");
        Logs.Insert(0, $"[{CurrentTime}] Runtime scan complete: {RuntimeState}.");
    }

    [RelayCommand]
    private async Task ExecuteCommandAsync()
    {
        var command = CommandText.Trim();
        if (command.Length == 0)
            return;

        Conversation.Add($"YOU: {command}");
        Logs.Insert(0, $"[{DateTime.Now:HH:mm:ss}] COMMAND: {command}");

        if (command.Equals("status", StringComparison.OrdinalIgnoreCase))
        {
            await RefreshAsync();
            Conversation.Add($"HYDRA: Runtime state is {RuntimeState}.");
        }
        else if (command.Equals("voice on", StringComparison.OrdinalIgnoreCase))
        {
            VoiceState = "LISTENING";
            Conversation.Add("HYDRA: Voice subsystem armed.");
        }
        else if (command.Equals("voice off", StringComparison.OrdinalIgnoreCase))
        {
            VoiceState = "STANDBY";
            Conversation.Add("HYDRA: Voice subsystem suspended.");
        }
        else
        {
            Conversation.Add("HYDRA: Command queued for AI/automation routing.");
        }

        CommandText = string.Empty;
    }

    [RelayCommand]
    private void ClearLogs()
    {
        Logs.Clear();
        Logs.Add("[LOG] Buffer cleared.");
    }
}
