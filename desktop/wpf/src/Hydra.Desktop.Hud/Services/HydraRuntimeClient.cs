namespace Hydra.Desktop.Hud.Services;

public sealed record ModuleStatus(string Name, string State, string Detail, bool Online);

public sealed class HydraRuntimeClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<ModuleStatus>> GetModulesAsync(
        CancellationToken cancellationToken = default)
    {
        var bridgeOnline = false;
        try
        {
            using var response = await httpClient.GetAsync("/health", cancellationToken);
            bridgeOnline = response.IsSuccessStatusCode;
        }
        catch
        {
            bridgeOnline = false;
        }

        return new[]
        {
            new ModuleStatus("BRIDGE", bridgeOnline ? "ONLINE" : "OFFLINE", "127.0.0.1:5075", bridgeOnline),
            new ModuleStatus("VOICE", "READY", "STT / TTS / INTENT", true),
            new ModuleStatus("CORE", "READY", "RUNTIME ORCHESTRATOR", true),
            new ModuleStatus("AI", "STANDBY", "PROVIDER NOT SELECTED", false),
            new ModuleStatus("SMART HOME", "STANDBY", "MQTT / TUYA / TAPO / BLE", false)
        };
    }
}
