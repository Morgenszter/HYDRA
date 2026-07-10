using System.Diagnostics;
using Hydra.Desktop.Core.Runtime;

namespace Hydra.Desktop.Infrastructure.Runtime;

public sealed class HydraBridgeLifecycleService(
    HydraBridgeLifecycleOptions options,
    IHydraRuntimeService runtimeService) : IHydraBridgeLifecycleService
{
    public async Task<HydraBridgeLifecycleResult> EnsureBridgeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return options.Mode switch
        {
            HydraBridgeLifecycleMode.Disabled =>
                HydraBridgeLifecycleResult.Failure("Bridge lifecycle management is disabled."),
            HydraBridgeLifecycleMode.UserProcess =>
                await EnsureUserProcessAsync(cancellationToken),
            HydraBridgeLifecycleMode.WindowsService =>
                EnsureWindowsService(),
            _ => HydraBridgeLifecycleResult.Failure($"Unsupported bridge lifecycle mode: {options.Mode}.")
        };
    }

    private async Task<HydraBridgeLifecycleResult> EnsureUserProcessAsync(CancellationToken cancellationToken)
    {
        var desktopDirectory = AppContext.BaseDirectory;
        var bridgePath = Path.GetFullPath(Path.Combine(desktopDirectory, options.BridgeExecutableRelativePath));

        if (!File.Exists(bridgePath))
        {
            return HydraBridgeLifecycleResult.Failure($"Bridge executable not found: {bridgePath}");
        }

        var bridgeProcessName = Path.GetFileNameWithoutExtension(bridgePath);
        var isRunning = Process.GetProcessesByName(bridgeProcessName).Any(process =>
        {
            try
            {
                return string.Equals(process.MainModule?.FileName, bridgePath, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        });

        if (isRunning)
        {
            return await WaitForBridgeHealthAsync("Bridge process is already running.", cancellationToken);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = bridgePath,
            WorkingDirectory = Path.GetDirectoryName(bridgePath) ?? desktopDirectory,
            UseShellExecute = false
        };

        Process.Start(startInfo);
        return await WaitForBridgeHealthAsync("Bridge process start requested.", cancellationToken);
    }

    private async Task<HydraBridgeLifecycleResult> WaitForBridgeHealthAsync(
        string startMessage,
        CancellationToken cancellationToken)
    {
        var timeout = TimeSpan.FromMilliseconds(Math.Max(500, options.HealthWaitTimeoutMs));
        var interval = TimeSpan.FromMilliseconds(Math.Max(100, options.HealthProbeIntervalMs));
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        string lastError = "Bridge did not report readiness.";

        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await runtimeService.GetSnapshotAsync(cancellationToken);
            if (result.IsSuccess)
            {
                return HydraBridgeLifecycleResult.Success($"{startMessage} Runtime health confirmed.");
            }

            lastError = result.ErrorMessage ?? lastError;
            await Task.Delay(interval, cancellationToken);
        }

        return HydraBridgeLifecycleResult.Failure($"{startMessage} Health confirmation timed out. Last error: {lastError}");
    }

    private HydraBridgeLifecycleResult EnsureWindowsService()
    {
        return HydraBridgeLifecycleResult.Failure(
            $"Windows Service mode selected for '{options.WindowsServiceName}', but service installation/control is not enabled in this build yet.");
    }
}