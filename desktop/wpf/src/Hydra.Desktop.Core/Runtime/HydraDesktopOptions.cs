namespace Hydra.Desktop.Core.Runtime;

public sealed class HydraDesktopOptions
{
    public string BridgeEndpoint { get; init; } = "https://localhost:5001";

    public string ClientId { get; init; } = "hydra-wpf-desktop";
}