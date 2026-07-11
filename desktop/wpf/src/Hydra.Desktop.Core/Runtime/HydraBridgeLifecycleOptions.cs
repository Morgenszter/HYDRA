namespace Hydra.Desktop.Core.Runtime;

public sealed class HydraBridgeLifecycleOptions
{
    public HydraBridgeLifecycleMode Mode { get; init; } = HydraBridgeLifecycleMode.UserProcess;

    public string BridgeExecutableRelativePath { get; init; } = "..\\Bridge\\Hydra.Bridge.Api.exe";

    public string WindowsServiceName { get; init; } = "HYDRA_HOME_Bridge";

    public int HealthWaitTimeoutMs { get; init; } = 5000;

    public int HealthProbeIntervalMs { get; init; } = 250;
}